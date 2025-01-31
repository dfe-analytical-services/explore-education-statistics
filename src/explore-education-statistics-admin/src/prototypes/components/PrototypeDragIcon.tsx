import React from 'react';

const DragIcon = ({ className }: { className?: string }) => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    version="1.1"
    width="32"
    height="32"
    viewBox="0 0 32 32"
    aria-hidden
    className={className}
  >
    <path d="M17.984 5.921v8.063h8v-4l6.016 6.016-6.016 6.015v-4.093h-8v8.063h4.031l-6.015 6.015-6.016-6.015h4v-8.063h-8v4.062l-5.984-5.984 5.984-5.984v3.968h8v-8.063h-3.906l5.922-5.921 5.922 5.921h-3.938z" />
  </svg>
);

export default DragIcon;
