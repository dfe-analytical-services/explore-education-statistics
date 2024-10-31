module ApiReferenceHelpers
  # @param [Openapi3Parser::Node::Schema] schema_data
  # @param [Array<Openapi3Parser::Node::Schema>] references
  # @return [*]
  def schema_example(schema_data, references = [])
    if schema_data.example
      return schema_data.example
    end

    if schema_data.default
      return schema_data.default
    end

    if is_primitive_schema(schema_data)
      return Utils::primitive_schema_example(schema_data)
    end

    if schema_data.type == "array"
      items = schema_data.items

      if items && items != schema_data
        if items.one_of&.any?
          return items
                   .one_of
                   .reject { |schema| references.include?(schema) }
                   # Add schema to references to avoid infinite recursion
                   .map { |schema| schema_example(schema, references + [schema]) }
        else
          return [schema_example(items)]
        end
      else
        return items ? [schema_example(items)] : []
      end
    end

    if schema_data.one_of&.any?
      schemas = schema_data
                  .one_of
                  .reject { |schema| references.include?(schema) }

      # Add schema to references to avoid infinite recursion
      return schema_example(schemas[0], references + [schema_data])
    end

    # Is extending a referenced another schema using allOf
    if schema_data.type.nil? && schema_data.all_of&.count == 1
      return schema_example(schema_data.all_of[0])
    end

    properties = get_schema_properties(schema_data)

    properties = properties.each_with_object({}) do |(name, schema), memo|
      memo[name] = schema_example(schema, references + [schema])
    end

    if schema_data.additional_properties?
      properties["<*>"] = if schema_data.additional_properties_schema
                            schema_example(schema_data.additional_properties_schema)
                          else
                            {}
                          end
    end

    properties
  end

  # @param [Openapi3Parser::Node::Schema] schema
  # @return [*]
  def get_schema_properties(schema)
    properties = schema.properties.to_h

    schema.all_of.to_a.each do |all_of_schema|
      properties.merge!(all_of_schema.properties.to_h)
    end

    properties
  end

  # @param [Openapi3Parser::Node::Schema] schema
  # @return [String]
  def render_schema_type(schema)
    if schema.one_of&.any?
      schemas = schema.one_of.map { |s| "<li>#{render_schema_type(s)}</li>" }
                      .join

      return "one of: <ul>#{schemas}</ul>"
    end

    if schema.type == "object"
      unless schema.name
        if schema.additional_properties? && schema.additional_properties_schema
          return "dictionary (#{render_schema_type(schema.additional_properties_schema)})"
        end

        return "object"
      end

      href = ::Middleman::Util::url_for(@app, "/schemas/#{schema.name}/index.html", {
        current_resource: current_resource
      })

      "<a href='#{href}'>#{schema.name}</a>"
    elsif schema.type == "array"
      items = schema.items

      if items.nil?
        return "array"
      end

      "array (#{render_schema_type(items)})"
    else
      if schema.name
        href = ::Middleman::Util::url_for(@app, "/schemas/#{schema.name}/index.html", {
          current_resource: current_resource
        })

        "<a href='#{href}'>#{schema.name}</a>"
      else
        # Make assumption that all oneOf items are same type as
        # Swashbuckle theoretically shouldn't allow different types.
        if schema.all_of&.any?
          return render_schema_type(schema.all_of[0])
        end

        schema.type || "any"
      end
    end
  end

  # @param [Openapi3Parser::Node::Schema] schema
  # @param [Openapi3Parser::Node::Schema, String] property
  # @return [Boolean]
  def is_required_schema_property?(schema, property)
    if schema.requires?(property)
      return true
    end

    if schema.all_of&.any?
      return schema.all_of.to_a.reduce(false) do |_, all_of_schema|
        all_of_schema.requires?(property)
      end
    end

    false
  end

  # Return true if the schema is a reference (i.e. a $ref).
  #
  # The OpenAPI parser automatically resolves references for us, but we sometimes
  # need to know if the schema was originally a reference.
  #
  # @param [Openapi3Parser::Node::Schema] schema
  # @return [Boolean]
  def is_referenced_schema(schema)
    schema.node_context.document_location != schema.node_context.source_location
  end

  # @param [Openapi3Parser::Node::Schema] schema
  # @return [Boolean]
  def is_complex_schema(schema)
    !!(schema.all_of || schema.any_of || schema.one_of || schema.not)
  end

  # @param [Openapi3Parser::Node::Schema] schema
  # @return [Boolean]
  def is_primitive_schema(schema)
    !is_complex_schema(schema) && !schema.type.nil? && schema.type != "object" && schema.type != "array"
  end

  # @return [Boolean]
  def has_schema_validations(schema)
      schema.min_length != 0 ||
      schema.max_length != nil ||
      schema.min_items != 0 ||
      schema.max_items != nil ||
      schema.min_properties != 0 ||
      schema.max_properties != nil ||
      schema.minimum != nil ||
      schema.maximum != nil ||
      schema.unique_items?
  end

  class Utils
    # @param [Openapi3Parser::Node::Schema] schema
    # @return [String, Number, Boolean]
    def self.primitive_schema_example(schema)
      if schema.example
        return schema.example
      end

      if schema.default
        return schema.default
      end

      case schema.type
      when "string"
        schema.format ? "string(#{schema.format})" : "string"
      when "number", "integer"
        0
      when "boolean"
        true
      else
        raise "Invalid primitive schema type: #{schema.type}"
      end
    end
  end
end
