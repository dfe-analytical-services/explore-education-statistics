import { ArrowLeft, ArrowRight } from '@common/components/ArrowIcons';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useDesktopMedia } from '@common/hooks/useMedia';
import styles from '@common/modules/table-tool/components/TableHeadersGroupControls.module.scss';
import useTableHeadersContext from '@common/modules/table-tool/contexts/TableHeadersContext';
import classNames from 'classnames';
import React from 'react';

interface Props {
  defaultNumberOfItems: number;
  groupName: string;
  id: string;
  index: number;
  isLastGroup: boolean;
  legend: string;
  showMovingControls: boolean;
  totalItems: number;
  onMoveAxis: () => void;
  onMoveDown: () => void;
  onMoveUp: () => void;
}

const TableHeadersGroupControls = ({
  defaultNumberOfItems,
  groupName,
  id,
  index,
  isLastGroup,
  legend,
  showMovingControls,
  totalItems,
  onMoveAxis,
  onMoveDown,
  onMoveUp,
}: Props) => {
  const {
    activeGroup,
    expandedLists,
    setActiveGroup,
    toggleExpandedList,
    toggleGroupDraggingEnabled,
    toggleReverseOrder,
    toggleMoveControlsActive,
  } = useTableHeadersContext();
  const { isMedia: isDesktopMedia } = useDesktopMedia();
  const isExpanded = expandedLists.includes(id);
  const displayItems = isExpanded ? totalItems : defaultNumberOfItems;
  const hasMoreItems = isExpanded || totalItems > displayItems;
  const disableControls = activeGroup && activeGroup !== id;

  return (
    <div className="govuk-!-padding-top-2">
      {activeGroup === id ? (
        <Button
          className="govuk-!-width-full govuk-!-margin-bottom-0"
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
                'govuk-!-width-full govuk-!-margin-bottom-6 govuk-!-text-align-centre',
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
          <ButtonGroup className="govuk-!-margin-bottom-0">
            {showMovingControls ? (
              <>
                {index !== 0 && (
                  <Button
                    disabled={!!disableControls}
                    variant="secondary"
                    onClick={onMoveUp}
                  >
                    {isDesktopMedia ? (
                      <>
                        <ArrowLeft />
                        <VisuallyHidden>{`Move ${legend} left`}</VisuallyHidden>
                      </>
                    ) : (
                      <>
                        <ArrowLeft className={styles.upDownArrow} />
                        Move<VisuallyHidden> {legend}</VisuallyHidden> up
                      </>
                    )}
                  </Button>
                )}
                {!isLastGroup && (
                  <Button
                    disabled={!!disableControls}
                    variant="secondary"
                    onClick={onMoveDown}
                  >
                    {isDesktopMedia ? (
                      <>
                        <ArrowRight />
                        <VisuallyHidden>{`Move ${legend} right`}</VisuallyHidden>
                      </>
                    ) : (
                      <>
                        <ArrowRight className={styles.upDownArrow} />
                        Move<VisuallyHidden> {legend}</VisuallyHidden> down
                      </>
                    )}
                  </Button>
                )}
                <Button
                  className="dfe-flex-grow--1"
                  disabled={!!disableControls}
                  variant="secondary"
                  onClick={onMoveAxis}
                >
                  Move<VisuallyHidden> {legend}</VisuallyHidden>
                  {` to ${
                    groupName.startsWith('rowGroups') ? 'columns' : 'rows'
                  }`}
                </Button>
                <Button onClick={() => toggleMoveControlsActive(id)}>
                  Done
                </Button>
              </>
            ) : (
              <>
                <Button
                  className="govuk-!-width-one-third"
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
                  className="govuk-!-width-one-third"
                  disabled={!!disableControls}
                  onClick={() => {
                    toggleMoveControlsActive(id);
                  }}
                >
                  Move<VisuallyHidden> {legend}</VisuallyHidden>
                </Button>
                <Button
                  className="govuk-!-width-one-third"
                  disabled={!!disableControls}
                  onClick={() => {
                    toggleReverseOrder(groupName);
                  }}
                >
                  Reverse order<VisuallyHidden> {legend}</VisuallyHidden>
                </Button>
              </>
            )}
          </ButtonGroup>
        </>
      )}
    </div>
  );
};

export default TableHeadersGroupControls;
