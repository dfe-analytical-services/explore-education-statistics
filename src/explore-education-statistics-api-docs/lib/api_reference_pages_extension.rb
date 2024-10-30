class ApiReferencePagesExtension < Middleman::Extension
  expose_to_template :api_url

  def initialize(app, options_hash = {}, &block)
    super

    @sitemap = app.sitemap
    @config = app.config[:tech_docs]
    @endpoint_template = Middleman::Util.normalize_path("/endpoints/template.html")
    @schema_template = Middleman::Util.normalize_path("/schemas/template.html")

    app.ignore @endpoint_template
    app.ignore @schema_template
  end

  # @param [List<Middleman::Sitemap::Resource>] resources
  # @return [List<Middleman::Sitemap::Resource>]
  def manipulate_resource_list(resources)
    api_docs_path = @config[:api_docs_path]

    if api_docs_path.nil?
      return resources
    end

    document = if uri?(api_docs_path)
                 Openapi3Parser.load_url(api_docs_path)
               elsif File.exist?(api_docs_path)
                 # Load api file and set existence flag.
                 Openapi3Parser.load_file(api_docs_path)
               else
                 raise "Unable to load `api_docs_path` from config/tech-docs.yml"
               end

    new_resources = []

    @base_url = document.servers[0]&.url || ""

    document.paths.each do |uri, http_methods|
      get_operations(http_methods).each do |http_method, operation|
        new_resources << create_endpoint_page(uri, http_method, operation)
      end
    end

    document.components.schemas.each do |_, schema|
      new_resources << create_schema_page(schema)
    end

    resources + new_resources
  end

  private

  # @param [String] uri
  # @param [String] http_method
  # @param [Openapi3Parser::Node::Operation] operation
  # @return [Middleman::Sitemap::ProxyResource]
  def create_endpoint_page(uri, http_method, operation)
    id = operation.operation_id

    Middleman::Sitemap::ProxyResource.new(
      @sitemap,
      Middleman::Util.normalize_path("/endpoints/#{id}/index.html"),
      @endpoint_template
    ).tap do |p|
      p.add_metadata locals: {
        title: operation.summary,
        url: api_url(uri),
        http_method: http_method.upcase,
        description: operation.description,
        parameters: operation.parameters,
        request_body: operation.request_body,
        responses: operation.responses
      }
    end
  end

  # @param [String] uri
  # @return [String]
  def api_url(uri = "")
    @base_url.chomp("/") + uri
  end

  private

  # @param [Openapi3Parser::Node::Schema] schema
  # @return [Middleman::Sitemap::ProxyResource]
  def create_schema_page(schema)
    name = schema.name

    Middleman::Sitemap::ProxyResource.new(
      @sitemap,
      Middleman::Util.normalize_path("/schemas/#{name}/index.html"),
      @schema_template
    ).tap do |p|
      p.add_metadata locals: {
        title: name,
        schema: schema,
      }
    end
  end

  # @param [Openapi3Parser::Node::PathItem] path
  # @return [Hash<String, Openapi3Parser::Node::PathItem>]
  def get_operations(path)
    {
      "get" => path.get,
      "put" => path.put,
      "post" => path.post,
      "delete" => path.delete,
      "patch" => path.patch,
    }.compact
  end

  # @param [String] string
  # @return [Boolean]
  def uri?(string)
    uri = URI.parse(string)
    %w[http https].include?(uri.scheme || "")
  rescue URI::BadURIError
    false
  rescue URI::InvalidURIError
    false
  end
end

Middleman::Extensions.register(:api_reference_pages, ApiReferencePagesExtension)
