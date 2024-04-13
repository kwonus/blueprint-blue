namespace BlueprintBlue.Model
{
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization;
    using Blueprint.Blue;
    using System.Text;

    public class QID
    {
        public UInt16 year { get; set; }
        public byte month { get; set; }
        public byte day { get; set; }
        public UInt32 sequence { get; set; }

        public static bool operator > (QID x, QID y) => x.year  > y.year
                                                    || (x.year == y.year && x.month  > y.month)
                                                    || (x.year == y.year && x.month == y.month && x.day  > y.day)
                                                    || (x.year == y.year && x.month == y.month && x.day == y.day && x.sequence  > y.sequence);
        public static bool operator < (QID x, QID y) => (y > x);

        public QID(UInt16 y, byte m, byte d, UInt32 seq)
        {
            this.year = y;
            this.month = m;
            this.day = d;
            this.sequence = seq;
        }
        public QID(UInt32? seq)
        {
            DateTime today = DateTime.Now;
            this.year = (UInt16)today.Year;
            this.month = (byte)today.Month;
            this.day = (byte)today.Day;
            this.sequence = seq.HasValue ? seq.Value : 0;

            if (seq == null)
            {
                string folder = this.AsYamlFolder();

                UInt32 previous = 0;
                if (Directory.Exists(folder))
                {
                    foreach (string file in from item in Directory.EnumerateFiles(folder, "*.yaml") orderby item.Length descending, item descending select Path.GetFileNameWithoutExtension(item))
                    {
                        try
                        {
                            previous = UInt32.Parse(file);
                            break;
                        }
                        catch
                        {
                            continue; // user might have save files in here, keep trying.
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(folder);
                }
                this.sequence = previous + 1;
            }
        }
        public QID() // for deserialization
        {
            this.year = 0;
            this.month = 0;
            this.day = 0;
            this.sequence = 0;
        }

        // TodayOnly looks like this: ("", parts[0])
        // Otherwise looks like this: (parts[0], parts[1])
        private QID(string[] parts) : this(parts.Length == 2 ? parts[0] : string.Empty,
                                           parts.Length == 2 ? parts[1] : parts.Length == 1 ? parts[0] : "0")
        {
            ;
        }
        private QID(string seq, bool todayOnly) : this(string.Empty, todayOnly ? seq : "0")
        {
            ;
        }
        public QID(string date, string seq)
        {
            bool valid = false;
            switch (date.Length)
            {
                case 8: valid = ParseLonghand(date);      break;
                case 3: valid = ParseShorthand(date);     break;
                case 1:
                case 2: valid = ParseThisMonthOnly(date); break;
                case 0: valid = PseudoParseTodayOnly();   break;
            }
            if (valid)
            {
                valid = seq.Length > 0;
            }
            if (valid)
            { 
                try
                {
                    this.sequence = UInt32.Parse(seq);
                }
                catch
                {
                    sequence = 0;
                }
            }
            if (!valid)
            {
                sequence = year = month = day = 0;
            }
        }
        public QID(string id) : this(id.Split("."))
        {
            ;
        }
        public string? AsTodayOnly()
        {
            DateTime today = DateTime.Now;

            if (this.year == today.Year && this.month == today.Month && this.day == today.Day)
            {
                string sequence = QID.Base36((int)this.sequence);

                if (sequence.Length > 0)
                {
                    if (sequence[0] >= '0' && sequence[0] <= '9') // this assures that today-only tags are not ambiguous with macro tags
                    {
                        return sequence;
                    }
                    return '0' + sequence;
                }
            }
            return null;
        }
        public string? AsCurrentMonthOnly()
        {
            DateTime today = DateTime.Now;

            if (this.year == today.Year && this.month == today.Month)
            {
                StringBuilder id = new StringBuilder(6);
                string day = this.day.ToString();
                id.Append(day);
                id.Append('.');
                string sequence = this.sequence.ToString();
                id.Append(this.sequence);
                return id.ToString();
            }
            // if today is 4/1/2024, then I can execute macros last month (e.g. 3/2/2024 as #2._, or 3/31/2024 as #31._
            if (this.year == today.Year && this.month == today.Month-1 && today.Day < this.day)
            {
                StringBuilder id = new StringBuilder(6);
                string day = this.day.ToString();
                id.Append(day);
                id.Append('.');
                string sequence = this.sequence.ToString();
                id.Append(this.sequence);
                return id.ToString();
            }
            // special case for january:
            // if today is 1/1/2024, then I can execute macros last month (e.g. 12/2/2023 as #2._, or 12/31/2023 as #31._
            if (this.year == today.Year-1 && this.month == 12 && today.Month == 1 && today.Day < this.day)
            {
                StringBuilder id = new StringBuilder(6);
                string day = this.day.ToString();
                id.Append(day);
                id.Append('.');
                string sequence = this.sequence.ToString();
                id.Append(this.sequence);
                return id.ToString();
            }
            return null;
        }
        public string AsShorthand()
        {
            if (this.year >= 2000 && this.month >= 1 && this.month <= 12 && this.day >= 1 && this.day <= 31)
            {
                StringBuilder id = new StringBuilder(6);

                int yy = this.year - 2020;
                id.Append(QID.Base36(yy));

                if (this.month < 10)
                    id.Append(this.month.ToString());
                else
                    switch(this.month)
                    {
                        case 10: id.Append('A'); break;
                        case 11: id.Append('B'); break;
                        case 12: id.Append('C'); break;
                    }
                if (this.day < 10)
                    id.Append(this.day.ToString());
                else
                    id.Append((char)((int) 'A' + this.day - 10));

                id.Append('.');
                id.Append(QID.Base36((int)this.sequence));

                return id.ToString();

            }
            return "000.0";
        }
        public string AsLonghand()
        {
            if (this.year >= 2000 && this.month >= 1 && this.month <= 12 && this.day >= 1 && this.day <= 31)
            {
                return this.AsDate() + "." + QID.Base36((int)this.sequence);
            }
            return "00000000.0";
        }
        public string AsYamlPath()
        {
            string folder = this.AsYamlFolder();
            return Path.Combine(folder,this.sequence.ToString() + ".yaml").Replace('\\', '/');
        }
        public string AsYamlFolder()
        {
            return Path.Combine(QContext.HistoryPath, this.year.ToString(), this.month.ToString(), this.day.ToString()).Replace('\\', '/');
        }
        public override string ToString()
        {
            DateTime today = DateTime.Now;
            if (today.Year - 10 < this.year)
                return this.AsShorthand();
            else if (today.Year - 10 > this.year)
                return this.AsLonghand();

            // otherwise, we are on the 10-year mark
            if (today.Month < this.month)
                return this.AsShorthand();
            else if (today.Month > this.month)
                return this.AsLonghand();

            // otherwise, we are on the very month of the 10-year mark
            if (today.Day < this.day)
                return this.AsShorthand();
            
            return this.AsLonghand();
        }
        public static string Today()
        {
            DateTime today = DateTime.Now;

            StringBuilder date = new StringBuilder(11);
            date.Append(today.Day.ToString("D2"));
            date.Append(' ');
            date.Append(QID.Months[today.Month]);
            date.Append(' ');
            date.Append(today.Year.ToString("D4"));

            return date.ToString();
        }
        // We never really [fully] care is a bad date passes thru (i.e. 2/31/202021) ...
        // reason: we will not deserialize invocation, because that date will not be in our history
        // therefore: anomoly will be detected downstream (this applies to all date processing in this class)
        public string AsDate()
        {
            if (this.year >= 2000 && this.month >= 1 && this.month <= 12 && this.day >= 1 && this.day <= 31)
            {
                StringBuilder date = new StringBuilder(11);
                date.Append(this.day.ToString("D2"));
                date.Append(' ');
                date.Append(QID.Months[this.month]);
                date.Append(' ');
                date.Append(this.year.ToString("D4"));

                return date.ToString();
            }
            return "99 XYZ 9999";
        }
        private bool PseudoParseTodayOnly()
        {
            DateTime today = DateTime.Now;
            this.year  = (UInt16)today.Year;
            this.month = (byte)today.Month;
            this.day   = (byte)today.Day;

            return true;
        }
        // We can parse [approximately] the last thirty days, by just supplying the day (year and month parts are not required)
        // If the day passed is later than today's day of the month, then the previous month is implied.
        private bool ParseThisMonthOnly(string tag)
        {
            DateTime today = DateTime.Now;
            this.year = (UInt16)today.Year;
            this.month = (byte)today.Month;

            try
            {
                this.day = byte.Parse(tag);
                if (this.day >= 1 && this.day <= 31)
                {
                    if (this.day > today.Day)
                    {
                        this.month --;
                        if (this.month == 0)
                        {
                            this.month = 12;
                            this.year--;
                        }
                    }
                    return true;
                }
            }
            catch
            {
                ;
            }
            return false;
        }
        private bool ParseShorthand(string date)
        {
            try
            {
                if (date.Length == 3)
                {
                    DateTime today = DateTime.Now;
                    this.year = (UInt16)((today.Year / 10) * 10);
                    this.year += (byte)(date[0] - '0');

                    // Base 16
                    if (date[1] >= '1' && date[1] <= '9')
                        this.month = (byte)(date[1] - '0');
                    else if (date[1] >= 'a' && date[1] <= 'c')
                        this.month = (byte)(10 + date[1] - 'a');
                    else if (date[1] >= 'A' && date[1] <= 'C')
                        this.month = (byte)(10 + date[1] - 'A');
                    else
                        return false;

                    // Base 36
                    this.day = QID.UnBase36(date[2]);
                    if (this.day > 31 || this.day < 1)
                        return false;
                }
                return (this.year >= 2000 && this.month >= 1 && this.month <= 12 && this.day >= 1 && this.day <= 31);
            }
            catch
            {
                return false;
            }
        }
        private bool ParseLonghand(string date)
        {
            return false;
        }
        public static readonly string[] Months = [string.Empty, "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

        public static byte UnBase36(char digit)
        {
            if (digit >= '0' && digit <= '9')
                return (byte)(digit - '0');
            if (digit >= 'A' && digit <= 'Z')
                return (byte)(10 + (digit - 'A'));
            if (digit >= 'a' && digit <= 'z')
                return (byte)(10 + (digit - 'a'));
            return byte.MaxValue;
        }
        public static int UnBase36(string digits)
        {
            bool positive = true;
            int i = 0;
            int accumulator = 0;
            foreach (char digit in digits)
            {
                if (i++ == 0 && digit == '-')
                    positive = false;
                else
                {
                    byte value = UnBase36(digit);
                    if (value != byte.MaxValue)
                    {
                        accumulator *= 36;
                        accumulator += value;
                    }
                }
            }
            return positive ? accumulator : -accumulator;
        }

        public static char Base36(byte digit, char defaultVal)
        {
            if (digit < 10)
                return (char)('0' + digit);
            if (digit < 36)
                return (char)('A' + (digit - 10));
            return defaultVal;
        }

        public static string Base36(int value)
        {
            bool minus = (value < 0);
            if (minus)
                value *= (-1);

            StringBuilder digits = new StringBuilder(3);

            for (int remainder = value; /**/; value /= 36)
            {
                byte modula = (byte)(remainder % 36);
                char digit = Base36(modula, '!');
                digits.Insert(0, digit);

                if (value < 36)
                    break;
            }
            if (minus)
                digits.Insert(0, '-');

            return digits.ToString();
        }
    }
}
