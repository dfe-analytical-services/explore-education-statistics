import React from 'react';

interface Props {
  crestFileName: string;
  lineColourHexCode: string;
  title: string;
}

export default function OrgLogoGov({
  crestFileName,
  lineColourHexCode,
  title,
}: Props) {
  return (
    <div>
      <div
        style={{
          borderLeft: `2px solid ${lineColourHexCode}`,
          background: `url(/assets/images/${crestFileName}) no-repeat 8px 0`,
          backgroundSize: 'auto 32px',
          paddingTop: '35px',
          paddingLeft: '8px',
        }}
      >
        <span>{title}</span>
      </div>
    </div>
  );
}
