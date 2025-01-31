module Helpers

  # @param [*] value
  # @return [String]
  def json_pretty(value)
    JSON.pretty_generate(value)
  end

  # Render markdown to HTML when working in non-Markdown
  # contexts e.g. a HTML fragment.
  #
  # @param [String] content
  # @return [String]
  def render_markdown(content)
    markdown = Redcarpet::Markdown.new(config[:markdown][:renderer])
    markdown.render(content)
  end

  def host_url
    config[:tech_docs][:host]
  end

  # @param [String] text
  # @return [String]
  def link_to_contact_md(text = "")
    email = config[:tech_docs][:contact_email]

    "[#{text.presence || email}](mailto:#{email})"
  end
end
