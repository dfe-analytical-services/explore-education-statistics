import React, { ReactNode } from 'react';
import Moment from 'react-moment';
 
interface Props {
  value: string,
}

const Date = ({ value }: Props) => (
    <Moment format="D MMMM YYYY">{value}</Moment>
);

export default Date;