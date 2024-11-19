require 'govuk_tech_docs'
require 'lib/api_reference_pages_extension'
require 'lib/helpers'
require 'lib/api_reference_helpers'
require 'lib/govuk_tech_docs/path_helpers'

# Check for broken links
require 'html-proofer'

GovukTechDocs.configure(self, livereload: { js_host: "localhost", host: "127.0.0.1" })

# Override config from environment variables

if ENV.has_key?("TECH_DOCS_HOST")
  config[:tech_docs][:host] = ENV["TECH_DOCS_HOST"] || config[:tech_docs][:host]
end

if ENV.has_key?("TECH_DOCS_PREVENT_INDEXING")
  config[:tech_docs][:prevent_indexing] = ENV["TECH_DOCS_PREVENT_INDEXING"]
end

if ENV.has_key?("TECH_DOCS_API_DOCS_PATH")
  config[:tech_docs][:api_docs_path] = ENV["TECH_DOCS_API_DOCS_PATH"]
end

helpers Helpers
helpers ApiReferenceHelpers
activate :api_reference_pages

activate :relative_assets
set :relative_links, true

after_build do |builder|
  begin
    HTMLProofer.check_directory(config[:build_dir],
      {
        :disable_external => true,
        :swap_urls => {
          config[:tech_docs][:host] => "",
        }
      }).run
  rescue RuntimeError => e
    abort e.to_s
  end
end
