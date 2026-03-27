import React from 'react';

interface Props {
  title: string;
  fileName: string;
}

export default function OrgLogoNonGov({ title, fileName }: Props) {
  return (
    <img
      src={`/api/assets/${fileName}`}
      alt={`Logo for ${title}`}
      style={{ maxHeight: '60px', maxWidth: 'max-content' }}
    />
  );
}
