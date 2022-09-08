import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import styles from '@common/modules/table-tool/components/TableHeadersGroupControls.module.scss';
import useTableHeadersContext from '@common/modules/table-tool/contexts/TableHeadersContext';
import classNames from 'classnames';
import React from 'react';

interface Props {
  defaultNumberOfItems: number;
  groupName: string;
  id: string;
  legend: string;
  totalItems: number;
  onMove?: () => void;
}

const TableHeadersGroupControls = ({
  defaultNumberOfItems,
  groupName,
  id,
  legend,
  totalItems,
  onMove,
}: Props) => {
  const {
    activeGroup,
    expandedLists,
    setActiveGroup,
    toggleExpandedList,
    toggleGroupDraggingEnabled,
  } = useTableHeadersContext();
  const isExpanded = expandedLists.includes(id);
  const displayItems = isExpanded ? totalItems : defaultNumberOfItems;
  const hasMoreItems = isExpanded || totalItems > displayItems;
  const disableControls = activeGroup && activeGroup !== id;

  return (
    <div className={styles.buttonsContainer}>
      {activeGroup === id ? (
        <Button
          onClick={() => {
            setActiveGroup(undefined);
            toggleGroupDraggingEnabled(true);
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
                  <VisuallyHidden>{` items for ${legend}`}</VisuallyHidden>
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
            onClick={() => {
              setActiveGroup(id);
              toggleGroupDraggingEnabled(false);
            }}
          >
            Reorder
            <VisuallyHidden>{` items in ${legend}`}</VisuallyHidden>
          </Button>
          <Button
            className={styles.moveButton}
            disabled={!!disableControls}
            onClick={onMove}
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
