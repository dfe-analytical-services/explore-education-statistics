import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import styles from '@common/modules/table-tool/components/TableHeadersGroupControls.module.scss';
import useTableHeadersContext from '@common/modules/table-tool/contexts/TableHeadersContext';
import classNames from 'classnames';
import React from 'react';

interface Props {
  groupName: string;
  id: string;
  legend: string;
  totalItems: number;
  onMove?: () => void;
}

const TableHeadersGroupControls = ({
  groupName,
  id,
  legend,
  totalItems,
  onMove,
}: Props) => {
  const {
    activeList,
    defaultNumberOfItems,
    expandedLists,
    setActiveList,
    toggleExpandedList,
    toggleGroupDraggingEnabled,
  } = useTableHeadersContext();
  const isExpanded = expandedLists.includes(id);
  const displayItems = isExpanded ? totalItems : defaultNumberOfItems;
  const hasMoreItems = isExpanded || totalItems > displayItems;
  const disableControls = activeList && activeList !== id;

  return (
    <div
      className={classNames(styles.buttonsContainer, {
        [styles.noShowMoreButton]: !hasMoreItems,
      })}
    >
      {activeList === id ? (
        <Button
          onClick={e => {
            e.preventDefault();
            setActiveList(undefined);
            toggleGroupDraggingEnabled();
          }}
        >
          Done
        </Button>
      ) : (
        <>
          {hasMoreItems && (
            <ButtonText
              ariaControls={id}
              ariaExpanded={isExpanded}
              className={classNames(
                styles.showMoreButton,
                'govuk-!-width-full govuk-!-margin-bottom-6 dfe-align--centre',
              )}
              disabled={!!disableControls}
              onClick={() => toggleExpandedList(id)}
            >
              {isExpanded ? (
                <>
                  Show fewer
                  <VisuallyHidden>{` ${legend} items`}</VisuallyHidden>
                </>
              ) : (
                <>
                  {`Show ${totalItems - displayItems} more`}
                  <VisuallyHidden>{` item${
                    totalItems - displayItems === 1 ? '' : 's'
                  } in  ${legend}`}</VisuallyHidden>
                </>
              )}
            </ButtonText>
          )}
          <Button
            disabled={!!disableControls}
            onClick={e => {
              e.preventDefault();
              setActiveList(id);
              toggleGroupDraggingEnabled();
            }}
          >
            Reorder
            <VisuallyHidden>{` items in ${legend}`}</VisuallyHidden>
          </Button>
          <Button
            className={styles.moveButton}
            disabled={!!disableControls}
            onClick={e => {
              e.preventDefault();
              onMove?.();
            }}
          >
            Move<VisuallyHidden> {legend}</VisuallyHidden>
            {` to ${groupName.startsWith('rowGroups') ? 'columns' : 'rows'}`}
          </Button>
        </>
      )}
    </div>
  );
};

export default TableHeadersGroupControls;
