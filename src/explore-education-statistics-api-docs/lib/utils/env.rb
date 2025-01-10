class EnvUtils

  # @param [String] key
  # @return [Boolean]
  def self.get_bool(key)
    case ENV[key]
    when "false", "f", "no", "n", "0", ""
      false
    else
      true
    end
  end
end
