import BasePagination, { PaginationProps } from '@common/components/Pagination';
import Link from '@frontend/components/Link';
import { LinkProps as RouterLinkProps } from 'next/link';
import { useRouter } from 'next/router';
import React from 'react';

interface Props extends Omit<PaginationProps, 'renderLink'> {
  scroll?: RouterLinkProps['scroll'];
  shallow?: RouterLinkProps['shallow'];
}

const Pagination = ({
  baseUrl,
  queryParams,
  scroll,
  shallow,
  ...props
}: Props) => {
  const router = useRouter();
  return (
    <BasePagination
      {...props}
      baseUrl={baseUrl ?? router.pathname}
      queryParams={queryParams ?? router.query}
      renderLink={({ 'data-testid': testId, ...linkProps }) => (
        <Link
          {...linkProps}
          testId={testId}
          scroll={scroll}
          shallow={shallow}
        />
      )}
    />
  );
};
export default Pagination;
