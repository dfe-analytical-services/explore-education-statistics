import ApiDataSetLocationCode from '@admin/pages/release/data/components/ApiDataSetLocationCode';
import { LocationOptionSource } from '@admin/services/apiDataSetVersionService';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import Pagination from '@common/components/Pagination';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import VisuallyHidden from '@common/components/VisuallyHidden';
import locationLevelsMap, {
  LocationLevelKey,
} from '@common/utils/locationLevelsMap';
import chunk from 'lodash/chunk';
import React, { useMemo, useState } from 'react';

interface Props {
  level: LocationLevelKey;
  options: LocationOptionSource[];
  pageSize?: number;
}

export default function ApiDataSetLocationGroupOptionsModal({
  level,
  options,
  pageSize = 10,
}: Props) {
  const [currentPage, setCurrentPage] = useState<number>(1);

  const optionChunks = useMemo(
    () => chunk(options, pageSize),
    [options, pageSize],
  );

  const totalPages = optionChunks.length;

  return (
    <Modal
      showClose
      title="View location group options"
      triggerButton={
        <ButtonText>
          View group options
          <VisuallyHidden>
            {' '}
            for {locationLevelsMap[level].plural}
          </VisuallyHidden>
        </ButtonText>
      }
    >
      <h3>Location group</h3>

      <SummaryList>
        <SummaryListItem term="Label">
          {locationLevelsMap[level].plural}
        </SummaryListItem>
      </SummaryList>

      <h3>Location group options</h3>

      <ul className="govuk-list" data-testid="location-group-options">
        {optionChunks[currentPage - 1].map((option, index) => {
          return (
            <li
              className="govuk-!-padding-bottom-2 govuk-!-padding-top-1 dfe-border-bottom"
              // eslint-disable-next-line react/no-array-index-key
              key={index}
            >
              {option.label}
              <ApiDataSetLocationCode location={option} />
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
