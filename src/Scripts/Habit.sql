INSERT INTO dev_habit.habits (
    id,
    name,
    description,
    type,
    frequency_type,
    frequency_times_per_period,
    target_value,
    target_unit,
    status,
    is_archived,
    end_date,
    milestone_target,
    milestone_current,
    created_at_utc,
    updated_at_utc,
    last_completed_at_utc
)
VALUES
    (
        'h_0196173a-f742-711f-89c4-53ec26a271bc',
        'Daily Meditation',
        'Morning mindfulness practice',
        2, 1, 1, 15, 'minutes', 1, FALSE,
        NULL, NULL, NULL,
        '2025-02-03T08:00:00Z', NULL, NULL
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271bd',
        'Read Every Day',
        'Read books to build a consistent learning habit',
        2, 1, 1, 20, 'pages', 1, FALSE,
        NULL, 1000, 120,
        '2025-02-04T08:00:00Z', NULL, '2025-02-10T08:00:00Z'
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271be',
        'Drink Water',
        'Stay hydrated throughout the day',
        2, 1, 8, 250, 'ml', 1, FALSE,
        NULL, NULL, NULL,
        '2025-02-05T08:00:00Z', '2025-02-07T08:00:00Z', '2025-02-10T15:30:00Z'
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271bf',
        'Morning Walk',
        'Get outside and move before work',
        2, 1, 1, 30, 'minutes', 1, FALSE,
        NULL, 100, 12,
        '2025-02-06T08:00:00Z', NULL, '2025-02-09T07:30:00Z'
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271c0',
        'Strength Training',
        'Complete a weekly strength workout',
        1, 2, 3, 1, 'workout', 1, FALSE,
        NULL, NULL, NULL,
        '2025-02-07T08:00:00Z', NULL, '2025-02-08T10:00:00Z'
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271c1',
        'Practice Spanish',
        'Practice vocabulary and listening',
        2, 1, 1, 15, 'minutes', 1, FALSE,
        NULL, 365, 18,
        '2025-02-08T08:00:00Z', NULL, '2025-02-10T18:00:00Z'
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271c2',
        'Journal',
        'Write a short reflection at the end of the day',
        1, 1, 1, 1, 'entry', 1, FALSE,
        NULL, NULL, NULL,
        '2025-02-09T08:00:00Z', NULL, NULL
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271c3',
        'Run',
        'Build endurance with regular runs',
        2, 2, 4, 5, 'km', 1, FALSE,
        NULL, 500, 42,
        '2025-02-10T08:00:00Z', NULL, '2025-02-10T06:45:00Z'
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271c4',
        'Plan the Week',
        'Review priorities and plan the upcoming week',
        1, 3, 1, 1, 'session', 1, FALSE,
        NULL, NULL, NULL,
        '2025-02-11T08:00:00Z', NULL, '2025-02-02T17:00:00Z'
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271c5',
        'No Screens Before Bed',
        'Avoid screens for 30 minutes before sleep',
        1, 1, 1, 1, 'day', 1, FALSE,
        NULL, NULL, NULL,
        '2025-02-12T08:00:00Z', NULL, '2025-02-09T22:00:00Z'
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271c6',
        'Learn Programming',
        'Spend focused time improving coding skills',
        2, 1, 1, 45, 'minutes', 1, FALSE,
        NULL, 100, 8,
        '2025-02-13T08:00:00Z', NULL, '2025-02-10T20:00:00Z'
    ),
    (
        'h_0196173a-f742-711f-89c4-53ec26a271c7',
        'Monthly Budget Review',
        'Review spending and update the monthly budget',
        1, 3, 1, 1, 'review', 1, FALSE,
        NULL, NULL, NULL,
        '2025-02-14T08:00:00Z', NULL, NULL
    );