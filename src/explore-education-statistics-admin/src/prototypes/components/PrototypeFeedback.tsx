import ButtonText from '@common/components/ButtonText';
import React, { useState } from 'react';

const Feedback = () => {
  const [initialView, setInitialView] = useState(true);
  const [usefulYes, setUsefulYes] = useState(false);
  const [usefulNo, setUsefulNo] = useState(false);

  const query = document.location.pathname;

  const checker = (feedback: string) => {
    if (feedback === 'yes') {
      setUsefulYes(true);
      setUsefulNo(false);
      setInitialView(false);
    } else if (feedback === 'no') {
      setUsefulYes(false);
      setUsefulNo(true);
      setInitialView(false);
    } else {
      setUsefulYes(false);
      setUsefulNo(false);
      setInitialView(true);
    }
  };

  return (
    <>
      {initialView && (
        <div>
          <p>Did you find this page useful?</p>
          <ButtonText
            onClick={() => {
              checker('yes');
            }}
          >
            yes
          </ButtonText>
          <ButtonText
            onClick={() => {
              checker('no');
            }}
          >
            no
          </ButtonText>
        </div>
      )}

      {usefulYes && (
        <>
          <ButtonText
            onClick={() => {
              checker('initial');
            }}
          >
            Close
          </ButtonText>
          <p>Thank you for your feedback</p>
        </>
      )}
      {usefulNo && (
        <>
          <ButtonText
            onClick={() => {
              checker('initial');
            }}
          >
            Close
          </ButtonText>
          <p>Please provide feedback {query} </p>
        </>
      )}
    </>
  );
};

export default Feedback;
