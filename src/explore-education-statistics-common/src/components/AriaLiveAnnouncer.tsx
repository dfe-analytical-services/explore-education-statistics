import React, { ReactNode, useState } from 'react';

interface Props {
  children: ReactNode;
}

export const AriaLiveContext = React.createContext({
  announceAssertive: (message: string) => {
    console.warn(message, "AriaLiveAnnounce hasn't wrapped the app");
  },
  announcePolite: (message: string) => {
    console.warn(message, "AriaLiveAnnounce hasn't wrapped the app");
  },
});

const AriaLiveAnnouncer = ({ children }: Props) => {
  const [assertiveMessage, setAssertiveMessage] = useState<
    string | undefined
  >();
  const [politeMessage, setPoliteMessage] = useState<string | undefined>();

  return (
    <AriaLiveContext.Provider
      value={{
        announceAssertive: message => {
          alert(message);
          setAssertiveMessage(message);
        },
        announcePolite: message => {
          alert(message);
          setPoliteMessage(message);
        },
      }}
    >
      {children}
      <div aria-live="assertive" className="govuk-visually-hidden">
        {assertiveMessage}
      </div>
      <div aria-live="polite" className="govuk-visually-hidden">
        {politeMessage}
      </div>
    </AriaLiveContext.Provider>
  );
};

export default AriaLiveAnnouncer;
