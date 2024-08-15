import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Pagination from '@common/components/Pagination';
import React, { useMemo, useState } from 'react';
import chunk from 'lodash/chunk';

const itemsPerPage = 10;

interface Props {
  id: string;
  label: string;
  modalLabel: string;
  options: string[];
}

export default function ApiDataSetMappableFilterColumnOptionsModal({
  id,
  label,
  options,
  modalLabel,
}: Props) {
  const [currentPage, setCurrentPage] = useState<number>(1);

  const optionChunks = useMemo(() => chunk(options, itemsPerPage), [options]);

  const totalPages = optionChunks.length;

  return (
    <Modal
      showClose
      title="View filter options"
      triggerButton={<ButtonText>View filter options</ButtonText>}
    >
      <h3>{modalLabel}</h3>
      <SummaryList>
        <SummaryListItem term="Label">{label}</SummaryListItem>
        <SummaryListItem term="ID">{id}</SummaryListItem>
      </SummaryList>

      <h3>Filter options</h3>

      <ul className="govuk-list">
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
