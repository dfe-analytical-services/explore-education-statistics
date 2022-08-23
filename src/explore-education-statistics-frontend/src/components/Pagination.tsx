import BasePagination, { PaginationProps } from '@common/components/Pagination';
import Link from '@frontend/components/Link';
import React from 'react';

const Pagination = ({ ...props }: Omit<PaginationProps, 'renderLink'>) => (
  <BasePagination
    {...props}
    renderLink={({ ...linkProps }) => <Link {...linkProps} />}
  />
);
export default Pagination;
