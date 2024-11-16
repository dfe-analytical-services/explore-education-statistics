class ApiReferencePagesExtension < Middleman::Extension
  expose_to_template :api_url

  def initialize(app, options_hash = {}, &block)
    super

    @sitemap = app.sitemap
    @config = app.config[:tech_docs]
    @base_template_path = "/templates/reference"
  end

  # @param [List<Middleman::Sitemap::Resource>] resources
  # @return [List<Middleman::Sitemap::Resource>]
  def manipulate_resource_list(resources)
    FileList.glob("source/openapi-v*.json").each do |path|
      document = Openapi3Parser.load_file(path)
      version = "v#{document.info.version}"

      if path != "source/openapi-#{version}.json"
        raise "OpenAPI document path '#{path}' does not match its version, expected 'source/openapi-#{version}.json'"
      end

      base_path = "/reference-#{version}"

      resources << Middleman::Sitemap::ProxyResource.new(
        @sitemap,
        Middleman::Util.normalize_path("#{base_path}/index.html"),
        Middleman::Util.normalize_path("#{@base_template_path}/index.html")
      ).tap do |p|
        p.add_metadata locals: {
          title: "API #{version} reference",
          version: version,
        }, page: {
          title: "API #{version} reference",
        }
      end

      resources << Middleman::Sitemap::ProxyResource.new(
        @sitemap,
        Middleman::Util.normalize_path("#{base_path}/endpoints/index.html"),
        Middleman::Util.normalize_path("#{@base_template_path}/endpoint_index.html")
      ).tap do |p|
        p.add_metadata locals: {
          title: "API #{version} endpoints",
          version: version,
        }, page: {
          title: "Endpoints",
        }
      end

      resources << Middleman::Sitemap::ProxyResource.new(
        @sitemap,
        Middleman::Util.normalize_path("#{base_path}/schemas/index.html"),
        Middleman::Util.normalize_path("#{@base_template_path}/schema_index.html")
      ).tap do |p|
        p.add_metadata locals: {
          title: "API #{version} schemas",
          version: version,
        }, page: {
          title: "Schemas"
        }
      end

      document.paths.each do |uri, http_methods|
        get_operations(http_methods).each do |http_method, operation|
          resources << create_endpoint_page(uri, http_method, operation, version, base_path)
        end
      end

      document.components.schemas.each do |_, schema|
        resources << create_schema_page(schema, version, base_path)
      end
    end

    resources
  end

  private

  # @param [String] uri
  # @return [String]
  def api_url(uri = "")
    @config[:api_url].chomp("/") + uri
  end

  # @param [String] uri
  # @param [String] http_method
  # @param [Openapi3Parser::Node::Operation] operation
  # @param [String] version
  # @param [String] base_path
  # @return [Middleman::Sitemap::ProxyResource]
  def create_endpoint_page(uri, http_method, operation, version, base_path)
    id = operation.operation_id

    Middleman::Sitemap::ProxyResource.new(
      @sitemap,
      Middleman::Util.normalize_path("#{base_path}/endpoints/#{id}/index.html"),
      Middleman::Util.normalize_path("#{@base_template_path}/endpoint.html")
    ).tap do |p|
      p.add_metadata locals: {
        title: operation.summary,
        version: version,
        url: api_url(uri),
        http_method: http_method.upcase,
        endpoint_description: operation.description,
        parameters: operation.parameters,
        request_body: operation.request_body,
        responses: operation.responses
      }
    end
  end


  # @param [Openapi3Parser::Node::Schema] schema
  # @param [String] version
  # @param [String] base_path
  # @return [Middleman::Sitemap::ProxyResource]
  def create_schema_page(schema, version, base_path)
    name = schema.name

    Middleman::Sitemap::ProxyResource.new(
      @sitemap,
      Middleman::Util.normalize_path("#{base_path}/schemas/#{name}/index.html"),
      Middleman::Util.normalize_path("#{@base_template_path}/schema.html")
    ).tap do |p|
      p.add_metadata locals: {
        title: name,
        version: version,
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
