import React from 'react';

const ChevronIcon = ({
  className,
  width = '1.2em',
  direction,
}: {
  className?: string;
  direction?: 'ascending' | 'descending';
  width?: string;
}) => (
  <svg
    aria-hidden
    className={className}
    focusable="false"
    viewBox="0 0 22 22"
    width={width}
    fill="currentColor"
  >
    {direction === 'descending' && (
      <path d="M15.4375 7L11 15.8687L6.5625 7L15.4375 7Z" />
    )}
    {direction === 'ascending' && (
      <path d="M6.5625 15L11 6.1313L15.4375 15L6.5625 15Z" />
    )}
    {!direction && (
      <>
        <path d="M8.1875 9.5L10.9609 3.95703L13.7344 9.5H8.1875Z" />
        <path d="M13.7344 12.0781L10.9609 17.6211L8.1875 12.0781H13.7344Z" />
      </>
    )}
  </svg>
);

export default ChevronIcon;
