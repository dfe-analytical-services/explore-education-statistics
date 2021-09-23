import sanitizeHtml, { SanitizeHtmlOptions } from '@common/utils/sanitizeHtml';
import htmlParser from 'html-react-parser';
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

  const parsedContent = htmlParser(cleanHtml, {});

  return (
    <div className={classNames('dfe-content', className)} data-testid={testId}>
      {parsedContent}
    </div>
  );
};

export default ContentHtml;
