/* eslint-disable no-shadow,jsx-a11y/click-events-have-key-events,jsx-a11y/click-events-have-key-events */
import React from 'react';
import Details from '@common/components/Details';

import { LoginContext } from '@admin/components/Login';

import { ExtendedComment } from '@admin/services/publicationService';
import styles from './PrototypeEditableContentAddComment.module.scss';

const key = (() => {
  let keyValue = 0;
  return () => {
    // eslint-disable-next-line no-plusplus
    keyValue++;
    return keyValue;
  };
})();

interface Props {
  initialComments: ExtendedComment[];
}

const ContentResolveComment = ({ initialComments }: Props) => {
  const context = React.useContext(LoginContext);

  const [comments, setComments] = React.useState<ExtendedComment[]>(
    initialComments,
  );

  const resolveComment = (index: number) => {
    comments[index].state = 'resolved';
    comments[index].resolvedOn = new Date();
    comments[index].resolvedBy = context.user && context.user.name;
    setComments([...comments]);
  };

  return (
    <>
      <div className={styles.resolveComment}>
        <Details
          summary="View comments for section"
          className="govuk-!-margin-bottom-1"
        >
          {comments.map(
            ({ name, time, comment, state, resolvedOn, resolvedBy }, index) => (
              <div key={key()}>
                <h2 className="govuk-body-xs govuk-!-margin-0">
                  <strong>{`${name} ${time.toLocaleDateString()}`}</strong>
                </h2>
                <p className="govuk-body-xs govuk-!-margin-bottom-1 ">
                  {comment}
                </p>
                {state === 'open' && (
                  <button
                    type="button"
                    className="govuk-button"
                    onClick={() => resolveComment(index)}
                  >
                    Resolve
                  </button>
                )}
                {state === 'resolved' && (
                  <p className="govuk-body-xs govuk-!-margin-bottom-1 ">
                    <em>
                      Resolved {resolvedOn && resolvedOn.toLocaleDateString()}{' '}
                      by {resolvedBy}
                    </em>
                  </p>
                )}
                <hr />
              </div>
            ),
          )}
        </Details>
      </div>
    </>
  );
};

export default ContentResolveComment;
