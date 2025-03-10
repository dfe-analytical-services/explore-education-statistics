module GovukTechDocs
  class SourceUrls
    def repo_path
      path = config[:tech_docs][:github_repo_path] || ""

      if path.empty?
        return ""
      end

      path.ends_with?("/") ? path : "#{path}/"
    end

    # Monkey patch to fix incorrect link to source file
    # as the API docs don't live in the project root.
    def source_from_file
      "#{repo_url}/blob/#{repo_branch}/#{repo_path}source/#{current_page.file_descriptor[:relative_path]}"
    end
  end
end
