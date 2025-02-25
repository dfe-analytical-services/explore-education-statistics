require 'dotenv'
require 'govuk_tech_docs'
require 'lib/utils/env'
require 'lib/api_reference_pages_extension'
require 'lib/helpers'
require 'lib/api_reference_helpers'
require 'lib/govuk_tech_docs/contribution_banner'
require 'lib/govuk_tech_docs/path_helpers'

# Check for broken links
require 'html-proofer'

Dotenv.load('.env')

GovukTechDocs.configure(self, livereload: { js_host: "localhost", host: "127.0.0.1" })

# Override config from environment variables

if ENV.has_key?("TECH_DOCS_HOST")
  config[:tech_docs][:host] = ENV["TECH_DOCS_HOST"]
end

if ENV.has_key?("TECH_DOCS_PREVENT_INDEXING")
  config[:tech_docs][:prevent_indexing] = EnvUtils.get_bool("TECH_DOCS_PREVENT_INDEXING")
end

if ENV.has_key?("TECH_DOCS_API_URL")
  config[:tech_docs][:api_url] = ENV["TECH_DOCS_API_URL"]
end

ignore "**/template.html"
ignore "partials/*"
ignore "endpoints/*"
ignore "templates/*"

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
