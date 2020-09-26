import Tag from '@common/components/Tag';
import React from 'react';

export interface StatusBlockProps {
  className?: string;
  color?: 'blue' | 'orange' | 'red' | 'green';
  text: string;
  id?: string | undefined;
}

const StatusBlock = ({
  color,
  text,
  className,
  id = undefined,
}: StatusBlockProps) => {
  return (
    <Tag colour={color} className={className} strong id={id}>
      {text}
    </Tag>
  );
};

export default StatusBlock;
