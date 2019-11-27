import React, { ReactNode, useState } from 'react';

interface Props {
  children: ReactNode;
}

export interface LiveMessage {
  id: string;
  type: 'polite' | 'assertive';
  message: string;
}

export const AriaLiveContext = React.createContext({
  announceMessage: (liveMessage: LiveMessage) => {
    // eslint-disable-next-line no-console
    console.warn(
      liveMessage.message,
      "AriaLiveAnnounce hasn't wrapped the app",
    );
  },
  removeMessage: (messageId: string) => {
    // eslint-disable-next-line no-console
    console.warn(messageId, "AriaLiveAnnounce hasn't wrapped the app");
  },
});

const AriaLiveAnnouncer = ({ children }: Props) => {
  const [messageList, setMessageList] = useState<LiveMessage[]>([]);

  const announceMessage = (liveMessage: LiveMessage) => {
    setMessageList([...messageList, liveMessage]);
  };
  const removeMessage = (messageId: string) => {
    setMessageList([
      ...messageList.filter(message => message.id !== messageId),
    ]);
  };

  return (
    <AriaLiveContext.Provider
      value={{
        announceMessage,
        removeMessage,
      }}
    >
      {children}
      <div
        role="alert"
        aria-live="assertive"
        aria-atomic="false"
        aria-relevant="additions"
        className="govuk-visually-hidden"
      >
        {messageList
          .filter(message => message.type === 'assertive')
          .map(({ message, id }) => (
            <div id={id} key={id}>
              {message}
            </div>
          ))}
      </div>
      <div
        role="alert"
        aria-live="polite"
        aria-atomic="false"
        aria-relevant="additions"
        className="govuk-visually-hidden"
      >
        {messageList
          .filter(message => message.type === 'polite')
          .map(({ message, id }) => (
            <div id={id} key={id}>
              {message}
            </div>
          ))}
      </div>
    </AriaLiveContext.Provider>
  );
};

export default AriaLiveAnnouncer;
