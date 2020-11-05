import sanitizeHtml, { SanitizeHtmlOptions } from '@common/utils/sanitizeHtml';
import React, { useMemo } from 'react';

export interface SanitizeHtmlProps {
  className?: string;
  dirtyHtml: string;
  options?: SanitizeHtmlOptions;
  testId?: string;
}

const SanitizeHtml = ({
  className,
  dirtyHtml,
  options,
  testId,
}: SanitizeHtmlProps) => {
  const cleanHtml = useMemo(() => {
    return sanitizeHtml(dirtyHtml, options);
  }, [dirtyHtml, options]);

  return (
    <div
      className={className}
      data-testid={testId}
      // eslint-disable-next-line react/no-danger
      dangerouslySetInnerHTML={{ __html: cleanHtml }}
    />
  );
};

export default SanitizeHtml;
