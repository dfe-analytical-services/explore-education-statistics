import React from 'react';
import Tag, { TagProps } from '@common/components/Tag';

interface Props {
  isCompatible?: boolean;
}

export default function ApiCompatibilityTag({ isCompatible }: Props) {
  let colour: TagProps['colour'];
  let text = '';

  switch (isCompatible) {
    case true:
      colour = 'green';
      text = 'Yes';
      break;
    case false:
      colour = 'red';
      text = 'No';
      break;
    default:
      colour = 'orange';
      text = 'Not available';
      break;
  }

  return <Tag colour={colour}>{text}</Tag>;
}
