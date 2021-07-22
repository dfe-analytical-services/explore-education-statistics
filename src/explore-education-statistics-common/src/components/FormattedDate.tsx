import { format as formatter, isValid, parse, toDate } from 'date-fns';
import React from 'react';

interface Props {
  children: Date | number | string;
  className?: string;
  format?: string;
  testId?: string;
  parseFormat?: string;
}

const FormattedDate = ({
  children,
  className,
  format = 'd MMMM yyyy',
  testId,
  parseFormat,
}: Props) => {
  let parsedDate: Date;

  if (typeof children === 'string') {
    parsedDate = parseFormat
      ? parse(children, parseFormat, new Date())
      : new Date(children);
  } else {
    parsedDate = toDate(children);
  }

  if (!isValid(parsedDate)) {
    return null;
  }

  return (
    <time data-testid={testId} className={className}>
      {formatter(parsedDate, format)}
    </time>
  );
};

export default FormattedDate;
