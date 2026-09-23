import classNames from 'classnames';
import React from 'react';

export interface PlainTextContentProps {
  className?: string;
  testId?: string;
  text: string;
}

/**
 * Renders a plain text field, preserving the line breaks the author entered.
 *
 * Use this instead of `ContentHtml` for fields that hold plain text rather than HTML. Passing plain text
 * through `ContentHtml` both loses its line breaks and mangles any `<` or `&` it contains, as those are
 * parsed as markup.
 */
export default function PlainTextContent({
  className,
  testId,
  text,
}: PlainTextContentProps) {
  return (
    <div
      className={classNames('dfe-white-space--pre-wrap', className)}
      data-testid={testId}
    >
      {text}
    </div>
  );
}
