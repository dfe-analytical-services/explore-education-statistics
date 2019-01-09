import React from 'react';
import Moment from 'react-moment';

interface Props {
  value: string;
  className?: string;
}

const Date = ({ value, className }: Props) => (
  <Moment format="D MMMM YYYY" className={className}>
    {value}
  </Moment>
);

export default Date;
