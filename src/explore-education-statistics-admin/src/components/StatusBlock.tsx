import Tag from '@common/components/Tag';
import React from 'react';

export interface StatusBlockProps {
  className?: string;
  checklistStyle?: boolean;
  color?: 'blue' | 'orange' | 'red' | 'green';
  id?: string | undefined;
  newAdminStyle?: boolean;
  text: string;
}

const StatusBlock = ({
  className,
  checklistStyle = false,
  color,
  id = undefined,
  newAdminStyle = false,
  text,
}: StatusBlockProps) => {
  return (
    <Tag colour={color} className={className} strong id={id}>
      {text}
      {newAdminStyle && checklistStyle && color === 'green' && <span> ✓</span>}
      {newAdminStyle && checklistStyle && color === 'red' && <span> ✖</span>}
    </Tag>
  );
};

export default StatusBlock;
