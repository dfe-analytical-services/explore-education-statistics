require 'http'
require 'json'
require 'date'
require 'uri'

require_relative './page'
require_relative './notification/expired'
require_relative './notification/will_expire_by'

class Notifier
  def initialize(notification, pages_url, limit = -1)
    @notification = notification
    @pages_url = pages_url
    @limit = limit
  end

  def run
    message = build_message(matching_pages)

    return if message.nil?

    puts "== Message"
    puts message

    puts "SKIPPING POST: No notification channel is configured, this is a dry run"
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

  def build_message(pages)
    return nil if pages.empty?

    page_count = @limit == -1 ? pages.size : [pages.size, @limit].min
    notification_message = page_count == 1 ? @notification.singular_message : @notification.multiple_message
    number_of = notification_message % [page_count]

    page_lines = pages[0..page_count-1].map do |page|
      @notification.line_for(page)
    end

    <<~doc
      Please ensure that the Public API documentation is up to date. #{number_of}:

      #{page_lines.join("\n")}
    doc
  end

  private

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
