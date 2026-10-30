require 'http'
require 'json'
require 'date'
require 'uri'

require_relative './page'
require_relative './notification/expired'
require_relative './notification/will_expire_by'

class Notifier
  def initialize(notification, pages_url, teams_webhook_url, live, limit = -1)
    @notification = notification
    @pages_url = pages_url
    @teams_webhook_url = teams_webhook_url
    @live = !!live
    @limit = limit
  end

  def run
    payload = build_payload(matching_pages)

    return if payload.nil?

    puts "== JSON Payload:"
    puts JSON.pretty_generate(payload)

    unless post_to_teams?
      puts "SKIPPING POST: Not posting anything, this is a dry run"
      return
    end

    response = HTTP.post(@teams_webhook_url, json: payload)

    unless response.status.success?
      raise "Unable to post to Teams: #{response.status} #{response.body}"
    end
  end

  def pages
    begin
      JSON.parse(HTTP.get(@pages_url)).map { |data|
        data['url'] = get_absolute_url(data['url'])
        Page.new(data)
      }
    rescue => exception
      warn "Notifier: could not get pages for tech docs at #{@pages_url}"
      warn exception.message
      return []
    end
  end

  def matching_pages
    pages
      .reject { |page| page.review_by.nil? }
      .select { |page| @notification.include?(page) }
      .sort_by { |page| page.review_by }
  end

  def build_payload(pages)
    return nil if pages.empty?

    page_count = @limit == -1 ? pages.size : [pages.size, @limit].min
    notification_message = page_count == 1 ? @notification.singular_message : @notification.multiple_message
    number_of = notification_message % [page_count]

    page_lines = pages[0..page_count-1].map do |page|
      @notification.line_for(page)
    end

    {
      type: "message",
      attachments: [{
        contentType: "application/vnd.microsoft.card.adaptive",
        content: {
          "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
          type: "AdaptiveCard",
          version: "1.4",
          body: [
            { type: "TextBlock", size: "Large", weight: "Bolder", text: "Please ensure that the Public API documentation is up to date", style: "heading", color: "attention", wrap: true },
            { type: "TextBlock", text: "#{number_of}:\n\n#{page_lines.join("\n")}", wrap: true }
          ],
          actions: [
            { type: "Action.OpenUrl", title: "View Public API documentation", url: docs_url }
          ]
        }
      }]
    }
  end

  def post_to_teams?
    @live
  end

  private

  def docs_url
    # pages.json lives at <docs root>/api/pages.json
    get_absolute_url("../")
  end

  def get_absolute_url url
    target_uri = URI(url)
    target_path = Pathname.new(target_uri.path)
    source_uri = URI(@pages_url)

    if target_path.relative?
      resulting_path = URI::join(source_uri, target_uri.path).path
    else
      resulting_path = target_uri.path
    end

    if source_uri.scheme == 'https'
      URI::HTTPS.build(scheme: source_uri.scheme, port: source_uri.port, host: source_uri.host, path: resulting_path).to_s
    else
      URI::HTTP.build(scheme: source_uri.scheme, port: source_uri.port, host: source_uri.host, path: resulting_path).to_s
end
  end
end
