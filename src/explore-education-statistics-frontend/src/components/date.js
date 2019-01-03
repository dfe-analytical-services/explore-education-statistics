import React, { Component } from 'react';
import Moment from 'react-moment';
 
export default class Date extends Component {
    render() {
        return (
            <div>
                <Moment format="D MMMM YYYY">{this.props.children}</Moment>
            </div>
        );
    }
}