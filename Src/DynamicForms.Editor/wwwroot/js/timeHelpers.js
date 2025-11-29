/**
 * Time Helper Functions for Dynamic Forms Editor
 * Provides utilities for formatting timestamps and relative time displays
 */

window.timeHelpers = {
    /**
     * Gets a human-readable relative time string from a DateTime
     * @param {string} dateTimeString - ISO 8601 datetime string
     * @returns {string} Relative time string (e.g., "2 minutes ago")
     */
    getRelativeTime: function (dateTimeString) {
        if (!dateTimeString) {
            return '';
        }

        const date = new Date(dateTimeString);
        const now = new Date();
        const diffMs = now - date;
        const diffSeconds = Math.floor(diffMs / 1000);
        const diffMinutes = Math.floor(diffSeconds / 60);
        const diffHours = Math.floor(diffMinutes / 60);
        const diffDays = Math.floor(diffHours / 24);
        const diffWeeks = Math.floor(diffDays / 7);
        const diffMonths = Math.floor(diffDays / 30);
        const diffYears = Math.floor(diffDays / 365);

        // Handle future dates
        if (diffMs < 0) {
            return 'in the future';
        }

        // Less than 10 seconds
        if (diffSeconds < 10) {
            return 'just now';
        }

        // Less than 1 minute
        if (diffSeconds < 60) {
            return `${diffSeconds} ${diffSeconds === 1 ? 'second' : 'seconds'} ago`;
        }

        // Less than 1 hour
        if (diffMinutes < 60) {
            return `${diffMinutes} ${diffMinutes === 1 ? 'minute' : 'minutes'} ago`;
        }

        // Less than 1 day
        if (diffHours < 24) {
            return `${diffHours} ${diffHours === 1 ? 'hour' : 'hours'} ago`;
        }

        // Less than 1 week
        if (diffDays < 7) {
            return `${diffDays} ${diffDays === 1 ? 'day' : 'days'} ago`;
        }

        // Less than 1 month
        if (diffWeeks < 4) {
            return `${diffWeeks} ${diffWeeks === 1 ? 'week' : 'weeks'} ago`;
        }

        // Less than 1 year
        if (diffMonths < 12) {
            return `${diffMonths} ${diffMonths === 1 ? 'month' : 'months'} ago`;
        }

        // Years
        return `${diffYears} ${diffYears === 1 ? 'year' : 'years'} ago`;
    },

    /**
     * Formats a DateTime to a short, readable format
     * @param {string} dateTimeString - ISO 8601 datetime string
     * @returns {string} Formatted date string (e.g., "Jan 15, 2024 14:30")
     */
    formatDateTime: function (dateTimeString) {
        if (!dateTimeString) {
            return '';
        }

        const date = new Date(dateTimeString);
        const options = {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        };

        return date.toLocaleString('en-US', options);
    },

    /**
     * Formats a DateTime to a short date format
     * @param {string} dateTimeString - ISO 8601 datetime string
     * @returns {string} Formatted date string (e.g., "Jan 15, 2024")
     */
    formatDate: function (dateTimeString) {
        if (!dateTimeString) {
            return '';
        }

        const date = new Date(dateTimeString);
        const options = {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        };

        return date.toLocaleDateString('en-US', options);
    },

    /**
     * Formats a DateTime to a short time format
     * @param {string} dateTimeString - ISO 8601 datetime string
     * @returns {string} Formatted time string (e.g., "14:30:45")
     */
    formatTime: function (dateTimeString) {
        if (!dateTimeString) {
            return '';
        }

        const date = new Date(dateTimeString);
        const options = {
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        };

        return date.toLocaleTimeString('en-US', options);
    },

    /**
     * Gets the current UTC timestamp as an ISO string
     * @returns {string} Current UTC timestamp
     */
    getCurrentUtcTimestamp: function () {
        return new Date().toISOString();
    },

    /**
     * Calculates the difference in milliseconds between two dates
     * @param {string} startDateTime - ISO 8601 datetime string
     * @param {string} endDateTime - ISO 8601 datetime string (defaults to now)
     * @returns {number} Difference in milliseconds
     */
    getTimeDifference: function (startDateTime, endDateTime) {
        const start = new Date(startDateTime);
        const end = endDateTime ? new Date(endDateTime) : new Date();
        return end - start;
    },

    /**
     * Checks if a date is today
     * @param {string} dateTimeString - ISO 8601 datetime string
     * @returns {boolean} True if the date is today
     */
    isToday: function (dateTimeString) {
        if (!dateTimeString) {
            return false;
        }

        const date = new Date(dateTimeString);
        const today = new Date();

        return date.getDate() === today.getDate() &&
            date.getMonth() === today.getMonth() &&
            date.getFullYear() === today.getFullYear();
    },

    /**
     * Starts a periodic update callback for relative time displays
     * @param {function} callback - Function to call on each interval
     * @param {number} intervalMs - Interval in milliseconds (default 30000)
     * @returns {number} Interval ID for clearing
     */
    startPeriodicUpdate: function (callback, intervalMs = 30000) {
        return setInterval(callback, intervalMs);
    },

    /**
     * Stops a periodic update
     * @param {number} intervalId - Interval ID from startPeriodicUpdate
     */
    stopPeriodicUpdate: function (intervalId) {
        if (intervalId) {
            clearInterval(intervalId);
        }
    },

    /**
     * Gets a compact relative time (for status bars)
     * @param {string} dateTimeString - ISO 8601 datetime string
     * @returns {string} Compact relative time (e.g., "2m ago", "1h ago")
     */
    getCompactRelativeTime: function (dateTimeString) {
        if (!dateTimeString) {
            return '';
        }

        const date = new Date(dateTimeString);
        const now = new Date();
        const diffMs = now - date;
        const diffSeconds = Math.floor(diffMs / 1000);
        const diffMinutes = Math.floor(diffSeconds / 60);
        const diffHours = Math.floor(diffMinutes / 60);
        const diffDays = Math.floor(diffHours / 24);

        if (diffSeconds < 10) return 'now';
        if (diffSeconds < 60) return `${diffSeconds}s ago`;
        if (diffMinutes < 60) return `${diffMinutes}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;
        if (diffDays < 30) return `${Math.floor(diffDays / 7)}w ago`;
        if (diffDays < 365) return `${Math.floor(diffDays / 30)}mo ago`;
        return `${Math.floor(diffDays / 365)}y ago`;
    }
};

// Export for module systems if available
if (typeof module !== 'undefined' && module.exports) {
    module.exports = window.timeHelpers;
}
