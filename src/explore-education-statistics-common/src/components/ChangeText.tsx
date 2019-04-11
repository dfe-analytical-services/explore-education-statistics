import React from 'react';

export interface ChangeTextProps {
  description: string;
  units: string;
  value: number;
}

const ChangeText = ({ description, units, value }: ChangeTextProps) => {
  let diff = 'same as';
  let marker = '\u2BC8';

  if (value > 0) {
    diff = 'higher than';
    marker = '\u2BC5';
  } else if (value < 0) {
    diff = 'lower than';
    marker = '\u2BC6';
  }

  const signedValue = value > 0 ? `+${value}` : value;

  return (
    <>
      {`${marker} ${
        units ? signedValue + units : signedValue
      } ${diff} ${description}`}
    </>
  );
};

export default ChangeText;
