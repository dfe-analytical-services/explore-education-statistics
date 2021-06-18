class KeywordListener:
    ROBOT_LISTENER_API_VERSION = 2
    
    def start_keyword(self, name, attributes):
        print(f'\t{attributes["kwname"]}   ${attributes["args"]}')