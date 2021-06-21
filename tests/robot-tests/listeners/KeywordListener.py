from logging import warn
from robot.running.context import EXECUTION_CONTEXTS

class KeywordListener:
    ROBOT_LISTENER_API_VERSION = 2
    
    def start_keyword(self, name, attributes):
        if attributes["type"].lower() == "keyword" and not attributes["libname"] in ["BuiltIn", "SeleniumLibrary"]:
            args = attributes["args"]
            values = EXECUTION_CONTEXTS.current.namespace.variables.replace_list(args)
            args_and_values = list(zip(args, values))
            args_and_value_string = "\t".join(map(lambda kvp: f'{kvp[0]}={kvp[1]}', args_and_values))
            print(f'\033[94m\t{attributes["source"]} line {attributes["lineno"]}\t{attributes["kwname"]}\t\t{args_and_value_string}')

    def end_keyword(self, name, attributes):
        if attributes["status"] == "FAIL":
            warn(f'\tFAILED KEYWORD: {attributes["kwname"]}\t\t{attributes["args"]}\t\t{attributes["source"]} line {attributes["lineno"]}')