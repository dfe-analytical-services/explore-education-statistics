import React, { useEffect, useState } from 'react';
import uuidv4 from 'uuid/v4';
import {
  AriaLiveContext,
  LiveMessage,
} from '@common/components/AriaLiveAnnouncer';

interface AriaLiveProps {
  message: string;
  setting?: 'polite' | 'assertive';
}

const AriaLiveMessage = ({ message, setting = 'assertive' }: AriaLiveProps) => {
  const [messageId] = useState<string>(uuidv4());
  const [messageSent, setMessageSent] = useState<boolean>(false);

  return (
    <AriaLiveContext.Consumer>
      {context => {
        return (
          <Message
            announceMessage={context.announceMessage}
            removeMessage={context.removeMessage}
            liveMessage={{ message, type: setting, id: messageId }}
            messageSent={messageSent}
            setMessageSent={setMessageSent}
          />
        );
      }}
    </AriaLiveContext.Consumer>
  );
};

interface LiveMessageProps {
  announceMessage: (liveMessage: LiveMessage) => void;
  removeMessage: (messageId: string) => void;
  liveMessage: LiveMessage;
  messageSent: boolean;
  setMessageSent: React.Dispatch<React.SetStateAction<boolean>>;
}

const Message = ({
  announceMessage,
  removeMessage,
  liveMessage,
  messageSent,
  setMessageSent,
}: LiveMessageProps) => {
  useEffect(() => {
    if (!messageSent) {
      announceMessage(liveMessage);
      setMessageSent(true);
    }

    return () => {
      removeMessage(liveMessage.id);
    };
  }, []);
  return null;
};

export default AriaLiveMessage;
