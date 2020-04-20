import React, { useMemo } from 'react';
import sanitizeHtml from '@common/utils/sanitizeHtml';

interface SanitizeHtmlProps {
  dirtyHtml: string;
}

const SanitizeHtml = ({ dirtyHtml }: SanitizeHtmlProps) => {
  const cleanHtml = useMemo(() => {
    return sanitizeHtml(dirtyHtml);
  }, [dirtyHtml]);

  return (
    <div
      // eslint-disable-next-line react/no-danger
      dangerouslySetInnerHTML={{ __html: cleanHtml }}
    />
  );
};

export default SanitizeHtml;
