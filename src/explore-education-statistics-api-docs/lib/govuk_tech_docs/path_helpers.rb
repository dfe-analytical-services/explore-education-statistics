module PatchedPathHelpers
  # Monkey patch this function as it doesn't correctly generate
  # paths when using relative links. This means that the TOC
  # won't open correctly on the current page as the paths are wrong.
  def get_path_to_resource(config, resource, current_page)
    if defined? app
      Middleman::Util::url_for(app, resource, { current_resource: current_page })
    else
      super
    end
  end
end

module GovukTechDocs
  module PathHelpers
    prepend PatchedPathHelpers
  end
end
