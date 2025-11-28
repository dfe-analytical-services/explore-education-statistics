import {
  format as formatter,
  formatRelative,
  isValid,
  parse,
  toDate,
} from 'date-fns';
import { toZonedTime } from 'date-fns-tz';
import React from 'react';
import { enGB } from 'date-fns/locale';

interface Props {
  children: Date | number | string;
  className?: string;
  format?: string;
  formatRelativeToNow?: boolean;
  testId?: string;
  parseFormat?: string;
  usingGmt?: boolean;
}

const FormattedDate = ({
  children,
  className,
  format = 'd MMMM yyyy',
  formatRelativeToNow = false,
  testId,
  parseFormat,
  usingGmt = false,
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

  const formatRelativeLocale = {
    lastWeek: "'last' eeee",
    yesterday: "'yesterday'",
    today: "'today at' h:mm a",
    tomorrow: "'tomorrow at' h:mm a",
    nextWeek: "'next' eeee",
  };
  const locale = {
    ...enGB,
    formatRelative: (token: string | number) =>
      formatRelativeLocale[token as keyof typeof formatRelativeLocale] ||
      format,
  };
  const dateFormattedRelativeToNow = usingGmt
    ? formatRelative(
        toZonedTime(parsedDate, 'Europe/London'),
        toZonedTime(new Date(), 'Europe/London'),
        { locale },
      )
    : formatRelative(parsedDate, new Date(), { locale });

  const dateFormatted = usingGmt
    ? formatter(toZonedTime(parsedDate, 'Europe/London'), format)
    : formatter(parsedDate, format);

  return (
    <time data-testid={testId} className={className}>
      {formatRelativeToNow ? dateFormattedRelativeToNow : dateFormatted}
    </time>
  );
};

export default FormattedDate;
