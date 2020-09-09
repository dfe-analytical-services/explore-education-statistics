import React, { useMemo } from 'react';
import sanitizeHtml from '@common/utils/sanitizeHtml';

interface SanitizeHtmlProps {
  className?: string;
  dirtyHtml: string;
}

const SanitizeHtml = ({ className, dirtyHtml }: SanitizeHtmlProps) => {
  const cleanHtml = useMemo(() => {
    return sanitizeHtml(dirtyHtml);
  }, [dirtyHtml]);

  return (
    <div
      className={className}
      // eslint-disable-next-line react/no-danger
      dangerouslySetInnerHTML={{ __html: cleanHtml }}
    />
  );
};

export default SanitizeHtml;
