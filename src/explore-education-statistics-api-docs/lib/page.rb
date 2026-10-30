require 'date'

class Page
  attr_reader :url, :title, :review_by

  def initialize(page_data)
    @url       = page_data["url"]
    @title     = page_data["title"]
    @review_by = page_data["review_by"].nil? ? nil : Date.parse(page_data["review_by"])
  end
end
