import { format as formatter, parse, toDate } from 'date-fns';
import React from 'react';

interface Props {
  children: Date | number | string;
  className?: string;
  format?: string;
  parseFormat?: string;
}

const FormattedDate = ({
  children,
  className,
  format = 'dd MMMM yyyy',
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

  return <time className={className}>{formatter(parsedDate, format)}</time>;
};

export default FormattedDate;
