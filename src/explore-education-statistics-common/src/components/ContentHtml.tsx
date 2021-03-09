import sanitizeHtml, { SanitizeHtmlOptions } from '@common/utils/sanitizeHtml';
import classNames from 'classnames';
import React, { useMemo } from 'react';

export interface ContentHtmlProps {
  className?: string;
  html: string;
  sanitizeOptions?: SanitizeHtmlOptions;
  testId?: string;
}

const ContentHtml = ({
  className,
  html,
  sanitizeOptions,
  testId,
}: ContentHtmlProps) => {
  const cleanHtml = useMemo(() => {
    return sanitizeHtml(html, sanitizeOptions);
  }, [html, sanitizeOptions]);

  return (
    <div
      className={classNames('dfe-content', className)}
      data-testid={testId}
      // eslint-disable-next-line react/no-danger
      dangerouslySetInnerHTML={{ __html: cleanHtml }}
    />
  );
};

export default ContentHtml;
