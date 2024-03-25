import Tag from '@common/components/Tag';
import classNames from 'classnames';
import capitalize from 'lodash/capitalize';
import React from 'react';

export type StatusBlockColors = 'blue' | 'orange' | 'red' | 'green';

export interface StatusBlockProps {
  className?: string;
  checklistStyle?: boolean;
  color?: StatusBlockColors;
  id?: string;
  text: string;
}

const StatusBlock = ({
  className,
  checklistStyle = false,
  color,
  id = undefined,
  text,
}: StatusBlockProps) => {
  return (
    <Tag
      colour={color}
      className={classNames('govuk-!-margin-bottom-1', className)}
      id={id}
    >
      {capitalize(text)}
      {checklistStyle && color === 'green' && <span aria-hidden> ✓</span>}
      {checklistStyle && color === 'red' && <span aria-hidden> ✖</span>}
    </Tag>
  );
};

export default StatusBlock;
