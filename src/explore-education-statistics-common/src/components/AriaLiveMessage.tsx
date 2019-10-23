/**
 * Consumes the AriaLiveAnnouncer context to be able to send messages that will be read to screenreaders
 */

import React from 'react';
import { AriaLiveContext } from '@common/components/AriaLiveAnnouncer';

interface Props {
  message: string;
  setting?: 'polite' | 'assertive';
}

const AriaLiveMessage = ({ message, setting = 'polite' }: Props) => {
  // send message to announcer

  return (
    <AriaLiveContext.Consumer>
      {context => {
        if (setting === 'polite') {
          context.announcePolite(message);
        } else {
          context.announceAssertive(message);
        }
        return null;
      }}
    </AriaLiveContext.Consumer>
  );
};

export default AriaLiveMessage;
