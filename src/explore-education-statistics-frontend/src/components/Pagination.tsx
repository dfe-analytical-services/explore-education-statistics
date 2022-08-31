import BasePagination, { PaginationProps } from '@common/components/Pagination';
import Link from '@frontend/components/Link';
import { useRouter } from 'next/router';
import React from 'react';

const Pagination = ({
  baseUrl,
  queryParams,
  ...props
}: Omit<PaginationProps, 'renderLink'>) => {
  const router = useRouter();
  return (
    <BasePagination
      {...props}
      baseUrl={baseUrl ?? router.pathname}
      queryParams={queryParams ?? router.query}
      renderLink={({ ...linkProps }) => <Link {...linkProps} />}
    />
  );
};
export default Pagination;
