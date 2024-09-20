import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Pagination from '@common/components/Pagination';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { useMemo, useState } from 'react';
import chunk from 'lodash/chunk';

interface Props {
  column: string;
  label: string;
  options: string[];
  pageSize?: number;
  publicId?: string;
}

export default function ApiDataSetMappableFilterColumnOptionsModal({
  column,
  label,
  options,
  pageSize = 10,
  publicId,
}: Props) {
  const [currentPage, setCurrentPage] = useState<number>(1);

  const optionChunks = useMemo(() => chunk(options, pageSize), [options]);

  const totalPages = optionChunks.length;

  return (
    <Modal
      showClose
      title="View filter options"
      triggerButton={
        <ButtonText>
          View filter options<VisuallyHidden> for {label}</VisuallyHidden>
        </ButtonText>
      }
    >
      <h3>Filter column</h3>
      <SummaryList>
        <SummaryListItem term="Label">{label}</SummaryListItem>
        <SummaryListItem term="Column">{column}</SummaryListItem>
        {publicId && (
          <SummaryListItem term="Identifier">{publicId}</SummaryListItem>
        )}
      </SummaryList>

      <h3>Filter options</h3>

      <ul className="govuk-list" data-testid="filter-options">
        {optionChunks[currentPage - 1].map((option, index) => {
          return (
            <li
              className="govuk-!-padding-bottom-2 govuk-!-padding-top-2 dfe-border-bottom"
              // eslint-disable-next-line react/no-array-index-key
              key={`option-${index}`}
            >
              {option}
            </li>
          );
        })}
      </ul>

      <Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        renderLink={({
          'aria-current': ariaCurrent,
          'aria-label': ariaLabel,
          'data-testid': testId,
          children,
          className,
          onClick,
        }) => (
          <ButtonText
            ariaCurrent={ariaCurrent}
            ariaLabel={ariaLabel}
            className={`govuk-link ${className}`}
            testId={testId}
            onClick={onClick}
          >
            {children}
          </ButtonText>
        )}
        onClick={setCurrentPage}
      />
    </Modal>
  );
}
