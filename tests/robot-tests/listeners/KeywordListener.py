from robot.running.context import EXECUTION_CONTEXTS
from tests.libs.logger import get_logger


class KeywordListener:
    ROBOT_LISTENER_API_VERSION = 2

    TEST_SUITE_KEYWORD_COLOUR = "\n\033[93m"

    COMMON_KEYWORD_COLOUR = "\033[94m\t"

    logger = get_logger(__name__)

    def start_keyword(self, name, attributes):

        source = attributes["source"]

        if (
            attributes["type"].lower() == "keyword"
            and not attributes["libname"] in ["BuiltIn", "SeleniumLibrary"]
            and source is not None
        ):
            args_and_value_string = self.get_args_and_values_string(attributes)
            logging_colour = (
                KeywordListener.COMMON_KEYWORD_COLOUR
                if "/libs/" in source
                else KeywordListener.TEST_SUITE_KEYWORD_COLOUR
            )
            print(
                f'{logging_colour}\t{attributes["kwname"]}\t\t{args_and_value_string}\t\t\t(from file://{attributes["source"]} line {attributes["lineno"]})'
            )

    def end_keyword(self, name, attributes):
        if attributes["status"] == "FAIL":
            args_and_value_string = self.get_args_and_values_string(attributes)
            self.logger.warn(
                f'\tFAILED KEYWORD: {attributes["kwname"]}\t\t{args_and_value_string}\t\tfile://{attributes["source"]} line {attributes["lineno"]}'
            )

    def get_args_and_values_string(self, attributes):

        args = attributes["args"]

        try:
            values = EXECUTION_CONTEXTS.current.namespace.variables.replace_list(args)
            args_and_values = list(zip(args, values))
            return ",\t".join(map(lambda kvp: f"{kvp[0]}={kvp[1]}", args_and_values))

        except BaseException:
            return ",\t".join(map(lambda arg: f"{arg}=Unknown", args))
