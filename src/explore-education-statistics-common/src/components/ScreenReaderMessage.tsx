import React, { useEffect, useState } from 'react';

/**
 * Announces changes to screen readers.
 * Use when just using aria-live on an existing element won't work
 * because it announces changes multiple times.
 */
export default function ScreenReaderMessage({ message }: { message: string }) {
  const [screenReaderMessage, setScreenReaderMessage] = useState('');

  useEffect(() => {
    // Clear the message then repopulate to ensure the new message is read,
    // and only read once.
    setScreenReaderMessage('');
    setTimeout(() => {
      setScreenReaderMessage(message);
    }, 200);
  }, [message]);

  return (
    <div
      aria-live="assertive"
      aria-atomic="true"
      aria-relevant="additions"
      className="govuk-visually-hidden"
    >
      {screenReaderMessage}
    </div>
  );
}
