import React from 'react';
import Tag from '@common/components/Tag';

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
    <Tag colour={color} className={className} strong id={id}>
      {text}
      {checklistStyle && color === 'green' && <span aria-hidden> ✓</span>}
      {checklistStyle && color === 'red' && <span aria-hidden> ✖</span>}
    </Tag>
  );
};

export default StatusBlock;
