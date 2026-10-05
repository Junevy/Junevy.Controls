namespace Junevy.Controls.Showcase
{
    /// <summary>
    /// 各演示区块的 XAML 源码片段（供 DemoSection 展示与复制）。
    /// 内容与各页演示保持一致，做演示级精简；修改页面演示时同步更新。
    /// </summary>
    public static class ShowcaseSnippets
    {
        // ---------------- 按钮控件（ButtonsPage） ----------------

        public const string ButtonBasics = """
            <jv:Button Content="确认" IsDefault="True" />
            <jv:Button Content="取消" />
            <jv:Button atc:Icon.Icon="&#xE611;" atc:Icon.IconSize="16" Content="刷新" />
            <jv:Button Content="禁用状态" IsEnabled="False" />
            <jv:Button Content="无边框样式" Style="{StaticResource NoBorderButtonStyle}" />

            <!-- 圆角经样式 Setter 设置 Border.CornerRadius 附加用法（逐实例 attribute 写法会被编译器拒绝） -->
            <Style x:Key="CornerRadius16Button" BasedOn="{StaticResource {x:Type jv:Button}}" TargetType="{x:Type jv:Button}">
                <Setter Property="Border.CornerRadius" Value="16" />
            </Style>
            <jv:Button Content="圆角 16（Border.CornerRadius）" Style="{StaticResource CornerRadius16Button}" />
            """;

        public const string ButtonShadow = """
            <!-- 常态阴影默认关闭（与 ComboBox 等同级控件贴平一致），需要浮起感时逐个开启 -->
            <jv:ComboBox Width="140" SelectedIndex="0">
                <jv:ComboBoxItem Content="Continuous" />
                <jv:ComboBoxItem Content="Trigger" />
            </jv:ComboBox>
            <jv:Button Content="默认贴平" />
            <jv:Button Content="ShowShadow=True" ShowShadow="True" />

            <!-- 或经样式批量开启（不影响悬停/按压反馈与尺寸） -->
            <Style BasedOn="{StaticResource {x:Type jv:Button}}" TargetType="{x:Type jv:Button}">
                <Setter Property="ShowShadow" Value="True" />
            </Style>
            """;

        public const string CardButtonDemo = """
            <jv:CardButton
                Title="在线相机"
                Content="12"
                MainColor="{DynamicResource Theme.Brush.Status.Success}"
                atc:Icon.FontFamily="{DynamicResource IconFont}"
                atc:Icon.Icon="&#xE66B;" />
            <jv:CardButton
                Title="待处理告警"
                Content="3"
                MainColor="{DynamicResource Theme.Brush.Status.Danger}"
                atc:Icon.Icon="&#xE932;" />
            """;

        public const string ToggleButtonDemo = """
            <!-- 开关需显式选择 SwitchToggleButton_Radius（胶囊）或 SwitchToggleButton_Rect（矩形）模板；
                 SwitchSize 为整体高度，轨道宽度按 2:1 自动推导 -->
            <jv:ToggleButton Content="自动曝光" IsChecked="True" SwitchSize="22"
                             Template="{StaticResource SwitchToggleButton_Radius}" />
            <jv:ToggleButton Content="自动对焦" IsChecked="False" SwitchSize="22"
                             Template="{StaticResource SwitchToggleButton_Radius}" />
            <jv:ToggleButton Content="矩形模板" IsChecked="True" SwitchSize="22"
                             Template="{StaticResource SwitchToggleButton_Rect}" />
            <jv:ToggleButton Content="禁用开关" IsChecked="True" IsEnabled="False" SwitchSize="22"
                             Template="{StaticResource SwitchToggleButton_Radius}" />
            """;

        public const string RadioButtonDemo = """
            <!-- GroupName 分组互斥；DisplayMode=Rectangular 切换方形标记 -->
            <jv:RadioButton Content="自动模式" GroupName="ShowcaseMode" IsChecked="True" />
            <jv:RadioButton Content="手动模式" GroupName="ShowcaseMode" />
            <jv:RadioButton Content="外部触发" GroupName="ShowcaseMode" />

            <jv:RadioButton Content="方形标记 A" DisplayMode="Rectangular" GroupName="ShowcaseShape" IsChecked="True" />
            <jv:RadioButton Content="方形标记 B" DisplayMode="Rectangular" GroupName="ShowcaseShape" />
            """;


        // ---------------- 输入与选择（InputsPage） ----------------

        public const string CheckBoxDemo = """
            <jv:CheckBox Content="启用检测" IsChecked="True" />
            <jv:CheckBox Content="未选中" />
            <jv:CheckBox Content="不确定态" IsChecked="{x:Null}" IsThreeState="True" />
            <jv:CheckBox Content="禁用" IsChecked="True" IsEnabled="False" />
            """;

        public const string TextBoxAssist = """
            <!-- 纯占位符：仅无值时显示（文本为空且未聚焦） -->
            <jv:TextBox Width="240" atc:PlaceholderAssist.Placeholder="相机名称（纯占位符）" />

            <!-- 图标 + 占位符 + 清空按钮 -->
            <jv:TextBox Width="240" ShowClear="True"
                        atc:Icon.FontFamily="{DynamicResource IconFont}"
                        atc:Icon.Icon="&#xE60C;"
                        atc:PlaceholderAssist.Placeholder="图标 + 占位符 + 清空" />

            <!-- 占位内容为 object：iconfont 字形时字体族跟随 atc:Icon.FontFamily -->
            <jv:TextBox Width="240" ShowClear="True"
                        atc:Icon.FontFamily="{DynamicResource IconFont}"
                        atc:Icon.Icon="&#xE60F;"
                        atc:PlaceholderAssist.Placeholder="&#xE60F; 检索设备（iconfont 占位符）" />

            <!-- 命令按钮（与清空互斥：ShowCommandButton=True 时需 ShowClear=False） -->
            <jv:TextBox Width="240"
                        ShowClear="False"
                        ShowCommandButton="True"
                        CommandButtonContent="&#xE611;"
                        CommandButtonCommand="{Binding TextBoxCommandButtonCommand}"
                        CommandButtonCommandParameter="来自 TextBox 命令按钮" />

            <!-- 原生 TextBox 经附加属性同样生效 -->
            <TextBox Width="240" atc:PlaceholderAssist.Placeholder="原生 TextBox 借用外观" />
            """;

        public const string TextBoxTitleAssist = """
            <!-- 四方位标题：Top（默认）/ Left / Bottom / Right，标题与输入框间距固定 4 DIP -->
            <jv:TextBox Width="220" atc:TitleAssist.Title="Server IP（Top）" />
            <jv:TextBox Width="220" atc:TitleAssist.Title="Server IP（Left）" atc:TitleAssist.TitlePlacement="Left" />
            <jv:ComboBox Width="220" atc:TitleAssist.Title="&#xE60F; Settings"
                         atc:TitleAssist.TitleFontFamily="{DynamicResource IconFont}"
                         atc:TitleAssist.TitleFontSize="14" />

            <!-- 必填标识：IsRequired=true 时标题旁显示图标，默认主题 Danger 色小圆点 -->
            <jv:TextBox Width="220" atc:TitleAssist.Title="服务器地址" atc:TitleAssist.IsRequired="True" />

            <!-- 自定义必填图标（如星号）与位置（标题左侧） -->
            <jv:ComboBox Width="220" atc:TitleAssist.Title="采集模式" atc:TitleAssist.IsRequired="True"
                         atc:TitleAssist.IsRequiredIcon="*" />
            <jv:TextBox Width="220" atc:TitleAssist.Title="设备名称" atc:TitleAssist.IsRequired="True"
                        atc:TitleAssist.IsRequiredIconPlacement="Left" />

            <!-- TitleWidth 固定标题区域宽度：表单式布局中不同长度的标题保持一致间距，整列对齐；
                 区域内靠齐由 TitleAlignment 决定（默认 Left），想要标题贴合输入框写 TitleAlignment="Right" -->
            <jv:TextBox Width="260" HorizontalAlignment="Left"
                        atc:TitleAssist.Title="用户名" atc:TitleAssist.TitlePlacement="Left" atc:TitleAssist.TitleWidth="110" />
            <jv:ComboBox Width="260" HorizontalAlignment="Left"
                         atc:TitleAssist.Title="电子邮箱地址" atc:TitleAssist.TitlePlacement="Left" atc:TitleAssist.TitleWidth="110" />
            """;

        public const string TitleAlignmentDemo = """
            <!-- 上下方位未设 TitleWidth：标题区域就是输入区整行宽，Right 即贴到输入框右端 -->
            <jv:TextBox Width="220" atc:TitleAssist.Title="默认（Left）" atc:TitleAssist.TitleAlignment="Left" />
            <jv:TextBox Width="220" atc:TitleAssist.Title="Right：贴到输入框右端" atc:TitleAssist.TitleAlignment="Right" />
            <jv:TextBox Width="220" atc:TitleAssist.Title="Center" atc:TitleAssist.TitleAlignment="Center" />
            <jv:TextBox Width="220" atc:TitleAssist.Title="Right + 必填" atc:TitleAssist.IsRequired="True"
                        atc:TitleAssist.TitleAlignment="Right" atc:TitleAssist.TitlePlacement="Bottom" />

            <!-- 左右方位未设 TitleWidth 时区域等于标题自身宽，靠齐无可见位移；配合 TitleWidth 才看得出三种靠齐。
                 必填标识随标题整组一起移动；TextBox / ComboBox / PasswordBox 三份模板行为一致 -->
            <jv:TextBox Width="280" HorizontalAlignment="Left"
                        atc:TitleAssist.Title="Left" atc:TitleAssist.TitleAlignment="Left"
                        atc:TitleAssist.TitlePlacement="Left" atc:TitleAssist.TitleWidth="180" />
            <jv:ComboBox Width="280" HorizontalAlignment="Left"
                         atc:TitleAssist.Title="Right（ComboBox）" atc:TitleAssist.TitleAlignment="Right"
                         atc:TitleAssist.TitlePlacement="Left" atc:TitleAssist.TitleWidth="180" />
            <jv:PasswordBox Width="280" HorizontalAlignment="Left"
                            atc:TitleAssist.Title="Center（PasswordBox）" atc:TitleAssist.TitleAlignment="Center"
                            atc:TitleAssist.TitlePlacement="Left" atc:TitleAssist.TitleWidth="180" />

            <!-- 旧版「左方位标题自动右对齐贴合输入框」的观感：显式写 Right 即可还原 -->
            <jv:TextBox Width="260" HorizontalAlignment="Left"
                        atc:TitleAssist.Title="用户名" atc:TitleAssist.TitleAlignment="Right"
                        atc:TitleAssist.TitlePlacement="Left" atc:TitleAssist.TitleWidth="110" />
            """;

        public const string PasswordBoxDemo = """
            <!-- 默认（Click）：右侧眼睛按钮点击切换明文；占位符、图标、标题与 jv:TextBox 同源 -->
            <jv:PasswordBox Width="240"
                            atc:Icon.FontFamily="{DynamicResource IconFont}"
                            atc:PlaceholderAssist.Placeholder="请输入密码" />

            <!-- 长按显示：按下即显示、松开立即隐藏 -->
            <jv:PasswordBox Width="240" RevealMode="PressAndHold"
                            atc:PlaceholderAssist.Placeholder="长按眼睛显示（PressAndHold）" />

            <!-- 错误泛红：IsError=true 时输入区背景微微发红，由业务验证失败时置位 -->
            <jv:PasswordBox Width="240" IsError="True"
                            atc:PlaceholderAssist.Placeholder="IsError=True 错误泛红" />

            <!-- Password 是真正的依赖属性，可直接绑定 ViewModel（原生 PasswordBox 做不到） -->
            <jv:PasswordBox Width="240" Password="{Binding DemoPassword}" />

            <!-- 密码错误状态可绑定，明文态 IsRevealed 亦可双向绑定 -->
            <jv:PasswordBox Width="240" IsError="{Binding PasswordInvalid}"
                            atc:TitleAssist.Title="登录密码" atc:TitleAssist.IsRequired="True" />
            """;

        public const string GroupBoxDemo = """
            <!-- 单击标题折叠/展开（IsCollapsible 默认 True）；IsCollapsed 初始折叠；
                 标题内交互元素不受点击折叠影响 -->
            <jv:GroupBox Header="默认：单击标题折叠 / 展开">
                <jv:TextBox Width="220" HorizontalAlignment="Left" atc:PlaceholderAssist.Placeholder="采集名称" />
            </jv:GroupBox>

            <jv:GroupBox Header="初始折叠（IsCollapsed=True）" IsCollapsed="True">
                <TextBlock Text="折叠后内容区域完全隐藏，不占布局空间。" />
            </jv:GroupBox>

            <jv:GroupBox Header="不可折叠（IsCollapsible=False）" IsCollapsible="False">
                <TextBlock Text="标题仅作展示，悬停无高亮。" />
            </jv:GroupBox>
            """;

        public const string ComboBoxDemo = """
            <!-- ItemsSource 绑定 -->
            <jv:ComboBox ItemsSource="{Binding Cameras}" SelectedIndex="0" />

            <!-- 占位符（未设置时 jv:ComboBox 保留历史默认文案） -->
            <jv:ComboBox atc:PlaceholderAssist.Placeholder="选择采集模式...">
                <jv:ComboBoxItem Content="Continuous" />
                <jv:ComboBoxItem Content="Trigger" />
                <jv:ComboBoxItem Content="Software" />
            </jv:ComboBox>

            <!-- 可编辑 + 占位符；原生 ComboBox 经附加属性提供占位 -->
            <jv:ComboBox IsEditable="True" atc:PlaceholderAssist.Placeholder="可编辑 + 占位符" />
            <ComboBox atc:PlaceholderAssist.Placeholder="原生 ComboBox 占位" />
            """;

        public const string DatePickerDemo = """
            <!-- 官方 DatePicker 主题接管；占位符是 DatePickerAssist.PlaceHolder（大写 H，string） -->
            <DatePicker atc:DatePickerAssist.PlaceHolder="选择开始日期" />
            <DatePicker SelectedDate="2026-09-20" atc:DatePickerAssist.PlaceHolder="选择结束日期" />
            """;

        public const string SliderDemo = """
            <!-- 数值框（ShowValueBox/ValueBoxSide/ValueFormatString）为 jv:Slider 专有；
                 回车或失焦提交，越界自动夹取，非法文本还原显示 -->
            <jv:Slider Maximum="100" Minimum="0" Value="60"
                       TickFrequency="10" TickPlacement="BottomRight"
                       ValueBoxSide="Right" ValueFormatString="F0" />

            <!-- 数值框在左（Left） -->
            <jv:Slider Maximum="16" Minimum="1" Value="8"
                       TickFrequency="1" TickPlacement="BottomRight"
                       ValueBoxSide="Left" ValueFormatString="F1" />

            <!-- 数值框在下（Bottom）；ShowValueBox=False 隐藏数值框；Orientation=Vertical 纵向 -->
            <jv:Slider Maximum="100" Minimum="0" Value="45" ValueBoxSide="Bottom" ValueFormatString="p0" />
            <jv:Slider Width="200" Maximum="100" Minimum="0" Value="30"
                       ShowValueBox="False" TickFrequency="10" TickPlacement="BottomRight" />
            <jv:Slider Height="180" Orientation="Vertical" Value="65" TickFrequency="10" TickPlacement="TopLeft" />
            """;


        // ---------------- 集合与数据（DataPage） ----------------

        public const string ListBoxVerticalDemo = """
            <jv:ListBox Height="180" ItemsSource="{Binding Devices}">
                <jv:ListBox.ItemTemplate>
                    <DataTemplate>
                        <Grid Margin="8,4">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto" />
                                <ColumnDefinition Width="*" />
                                <ColumnDefinition Width="Auto" />
                            </Grid.ColumnDefinitions>
                            <TextBlock Grid.Column="0" FontWeight="Bold" Text="{Binding Name}" />
                            <TextBlock Grid.Column="1" Margin="12,0,0,0"
                                       Foreground="{DynamicResource Theme.Brush.Text.Secondary}" Text="{Binding IP}" />
                            <TextBlock Grid.Column="2"
                                       Foreground="{DynamicResource Theme.Brush.Accent.Primary}" Text="{Binding Status}" />
                        </Grid>
                    </DataTemplate>
                </jv:ListBox.ItemTemplate>
            </jv:ListBox>
            """;

        public const string ListBoxHorizontalDemo = """
            <!-- Orientation=Horizontal 横向滑动；条目为自绘 Border 缩略卡 -->
            <jv:ListBox Height="110" ItemsSource="{Binding Tiles}" Orientation="Horizontal">
                <jv:ListBox.ItemTemplate>
                    <DataTemplate>
                        <Border Width="120" Margin="4"
                                Background="{DynamicResource Theme.Brush.Surface.Raised}"
                                BorderBrush="{DynamicResource Theme.Brush.Border.Default}"
                                BorderThickness="1" CornerRadius="8">
                            <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
                                <TextBlock HorizontalAlignment="Center" FontFamily="{DynamicResource IconFont}"
                                           FontSize="22" Foreground="{DynamicResource Theme.Brush.Accent.Primary}" Text="&#xE60C;" />
                                <TextBlock HorizontalAlignment="Center" FontSize="11"
                                           Foreground="{DynamicResource Theme.Brush.Text.Secondary}" Text="{Binding Name}" />
                            </StackPanel>
                        </Border>
                    </DataTemplate>
                </jv:ListBox.ItemTemplate>
            </jv:ListBox>
            """;

        public const string ListViewDemo = """
            <jv:ListView Height="180" ItemsSource="{Binding Devices}">
                <jv:ListView.View>
                    <GridView>
                        <GridViewColumn Width="160" DisplayMemberBinding="{Binding Name}" Header="设备" />
                        <GridViewColumn Width="100" DisplayMemberBinding="{Binding Status}" Header="状态" />
                        <GridViewColumn Width="180" DisplayMemberBinding="{Binding IP}" Header="IP 地址" />
                    </GridView>
                </jv:ListView.View>
            </jv:ListView>
            """;

        public const string DataGridDemo = """
            <jv:DataGrid Height="220" AutoGenerateColumns="False" IsReadOnly="True" ItemsSource="{Binding Results}">
                <jv:DataGrid.Columns>
                    <DataGridTextColumn Binding="{Binding Time}" Header="时间" />
                    <DataGridTextColumn Binding="{Binding Device}" Header="设备" />
                    <DataGridTextColumn Binding="{Binding Result}" Header="结果" />
                </jv:DataGrid.Columns>
            </jv:DataGrid>
            """;

        public const string DataGridEmptyTextDemo = """
            <!-- 附加属性：无数据时在表格中央显示空态文案 -->
            <jv:DataGrid Height="140" AutoGenerateColumns="False" IsReadOnly="True"
                         atc:DataGridAssist.EmptyText="暂无检测结果（空态演示）">
                <jv:DataGrid.Columns>
                    <DataGridTextColumn Binding="{Binding Time}" Header="时间" />
                    <DataGridTextColumn Binding="{Binding Device}" Header="设备" />
                    <DataGridTextColumn Binding="{Binding Result}" Header="结果" />
                </jv:DataGrid.Columns>
            </jv:DataGrid>
            """;


        // ---------------- 集合与数据（DataPage）· 分页 ----------------

        public const string PagingDemo = """
            <!-- PageSize 启用客户端分页：页脚自动出现（首页/滑窗页码/末页/每页条数/总数）；
                 Placement 切换页码栏方位 Bottom/Top/Left/Right（左/右为竖排页码栏）；
                 排序（列头点击）发生在分页之前，为全局排序语义；PageSize 置 0 恢复不分页 -->
            <jv:DataGrid ItemsSource="{Binding PagedRows}"
                         atc:PagingAssist.PageSize="10"
                         atc:PagingAssist.Placement="Bottom" />

            <!-- 纯图标导航用法：ListBox 隐藏标题或直接绑定任意条目集合 -->
            <jv:ListBox ItemsSource="{Binding PagedRows}"
                        atc:PagingAssist.PageSize="5"
                        atc:PagingAssist.Placement="Left" />

            <!-- 任意 ItemsControl 均可启用数据切片；页码控件也可独立摆放：
                 <jv:DataPager CurrentPage="{Binding Path=(atc:PagingAssist.CurrentPage), ElementName=List, Mode=TwoWay}"
                               PageCount="{Binding Path=(atc:PagingAssist.PageCount), ElementName=List}"
                               TotalCount="{Binding Path=(atc:PagingAssist.TotalCount), ElementName=List}"
                               PageSize="{Binding Path=(atc:PagingAssist.PageSize), ElementName=List, Mode=TwoWay}" /> -->
            """;


        // ---------------- 文本与状态（TextStatePage） ----------------

        public const string LabelDemo = """
            <!-- 各模式经样式触发器注入默认图标（atc:Icon.Icon），可用局部值覆盖；
                 图标为空时折叠图标区域，文字保持居中；Neutral 默认中性灰底，可用局部 Background 覆盖 -->
            <jv:Label Content="Error（默认）" DisplayMode="Error" />
            <jv:Label Content="Success" DisplayMode="Success" />
            <jv:Label Content="Warning" DisplayMode="Warning" />

            <jv:Label Content="BorderlessError" DisplayMode="BorderlessError" />
            <jv:Label Content="BorderlessWarning" DisplayMode="BorderlessWarning" />
            <jv:Label Content="BorderlessNotice" DisplayMode="BorderlessNotice" />
            <jv:Label Content="Neutral（默认灰底）" DisplayMode="Neutral" />
            <jv:Label Content="Neutral + 自定义图标" DisplayMode="Neutral" atc:Icon.Icon="&#xE651;" />
            """;

        public const string TextBlockDemo = """
            <!-- 左侧图标一律走 atc:Icon.* 附加属性（控件本身不承载 Content）：
                 atc:Icon.Icon 可为图标字体字符，也可为 Border/Image/Path 等任意元素；
                 为空时图标区整体折叠，且不留图文间距（5px 间距由图标右侧承载）。
                 图标字号走 atc:Icon.IconSize（库级默认 14，不随 FontSize 自动放大），着色走 atc:Icon.IconForeground。 -->
            <jv:TextBlock Width="320" FontSize="20" Text="Inspection Station"
                          atc:Icon.Icon="&#xE66B;" atc:Icon.IconSize="20" />
            <jv:TextBlock Width="320" FontSize="14" Text="无图标时标题不留残余缩进" />
            <jv:TextBlock Width="320" FontSize="16" Text="图标槽可放任意内容">
                <atc:Icon.Icon>
                    <Border Width="30" Height="30"
                            Background="{DynamicResource Theme.Brush.Accent.Primary}" CornerRadius="6">
                        <TextBlock HorizontalAlignment="Center" VerticalAlignment="Center"
                                   FontFamily="{DynamicResource IconFont}" FontSize="16"
                                   Foreground="{DynamicResource Theme.Brush.Text.OnAccent}" Text="&#xE66B;" />
                    </Border>
                </atc:Icon.Icon>
            </jv:TextBlock>
            """;

        public const string CodeEditorDemo = """
            <!-- 完全封装 AvalonEdit：公共 API 只暴露 Junevy 类型，高级场景经 InnerEditor 取内部实例。
                 SyntaxLanguage 运行时可切换：CSharp/VisualBasic/Cpp/Java/JavaScript/Html/Css/
                 Xml/Json/Sql/Python/Markdown/PowerShell；高亮取 AvalonEdit 内置规则私有副本，
                 着色统一映射 Theme.Brush.*，点击右上角按钮切换主题即可验证。 -->
            <jv:CodeEditor Height="320" SyntaxLanguage="CSharp" ShowLineNumbers="True" />
            """;


        // ---------------- 通知（NotifyPage） ----------------

        public const string BadgeDemo = """
            <!-- 包裹任意内容；Count 角标 / IsDot 红点 / MaxCount 上限 / OffsetX/Y 偏移 / Corner 角位 -->
            <jv:Badge Count="5">
                <jv:Button atc:Icon.FontFamily="{DynamicResource IconFont}" atc:Icon.Icon="&#xE932;" Content="消息" />
            </jv:Badge>
            <jv:Badge Count="1" IsDot="True">
                <jv:Button Content="更新" />
            </jv:Badge>
            <jv:Badge Count="99" MaxCount="9">
                <jv:Button Content="MaxCount=9" />
            </jv:Badge>
            <jv:Badge Count="3" OffsetX="-4" OffsetY="4">
                <jv:Button Content="偏移微调" />
            </jv:Badge>
            <jv:Badge Count="6" Corner="TopLeft">
                <Border Padding="10" Background="{DynamicResource Theme.Brush.Surface.Raised}"
                        BorderBrush="{DynamicResource Theme.Brush.Border.Default}" BorderThickness="1" CornerRadius="6">
                    <TextBlock FontFamily="{DynamicResource IconFont}" FontSize="18"
                               Foreground="{DynamicResource Theme.Brush.Text.Primary}" Text="&#xE66B;" />
                </Border>
            </jv:Badge>
            """;

        public const string MessageBarInlineDemo = """
            <!-- 布局内直接声明；Timeout=0 禁用自动关闭；显隐经 IsShown / Show() / Hide() 控制 -->
            <jv:MessageBar x:Name="InlineBar" Appearance="Warning" IsShown="True" Timeout="0"
                           Title="低曝光" Message="场景亮度低于目标值，请检查光源。" />
            <jv:Button Click="OnInlineShowClick" Content="Show()" />
            <jv:Button Click="OnInlineHideClick" Content="Hide()" />

            <!-- 代码控制：InlineBar.Show() / InlineBar.Hide() -->
            """;

        public const string MessageBarServiceDemo = """
            <!-- 弹出宿主（jv:MessageBarPresenter）注册在主窗口底部，不占用页面布局 -->
            <jv:Button Click="OnServiceInfoClick" Content="Informational" />
            <jv:Button Click="OnServiceSuccessClick" Content="Success" />
            <jv:Button Click="OnServiceWarningClick" Content="Warning" />
            <jv:Button Click="OnServiceDangerClick" Content="Danger（5 秒超时）" />
            <jv:Button Click="OnServiceClearClick" Content="Clear()" />
            """;

        public const string ToolTipDemo = """
            <!-- 合并库 Generic.xaml 后，任意元素的 ToolTip 自动获得主题样式 -->
            <TextBlock Text="悬浮到我这里查看原生 ToolTip"
                       ToolTip="取值范围 1.0 - 16.0，调整后立即生效" />
            <jv:Button Content="按钮 ToolTip" ToolTip="jv:Button 上的 ToolTip 同样获得主题样式" />
            <jv:CheckBox Content="复选框 ToolTip" ToolTip="任意 FrameworkElement 的 ToolTip 属性" />
            """;


        // ---------------- 菜单与导航（MenusPage） ----------------

        public const string SideMenuDemo = """
            <!-- DisplayMode=Horizontal：图标与标题横向排列（分类导航常用形态） -->
            <jv:SideMenu Width="240" ItemHeight="40" SelectionChanged="OnSideMenuSelectionChanged"
                         atc:Icon.FontFamily="{DynamicResource IconFont}" atc:Icon.IconSize="18">
                <jv:MenuItem Icon="&#xE66B;" Title="数据采集" />
                <jv:MenuItem Icon="&#xE646;" Title="报表中心" />
                <jv:MenuItem Icon="&#xE60A;" Title="系统设置" />
            </jv:SideMenu>

            <!-- DisplayMode=Vertical + Orientation=Vertical：图标在上、标题在下的紧凑图标栏 -->
            <jv:SideMenu Width="60" Orientation="Vertical" DisplayMode="Vertical" ItemHeight="52">
                <jv:MenuItem Icon="&#xE66B;" Title="采集" />
                <jv:MenuItem Icon="&#xE646;" Title="报表" />
            </jv:SideMenu>
            """;

        public const string MenuBarDemo = """
            <!-- 原生 <Menu> 顶栏样式：JunevyMenuBarStyle + JunevyMenuBarItemStyle，
                 子级菜单项自动交给 JunevyContextMenuItemStyle 渲染 -->
            <Menu Style="{StaticResource JunevyMenuBarStyle}">
                <MenuItem Header="文件">
                    <MenuItem Header="新建" InputGestureText="Ctrl+N" />
                    <MenuItem Header="打开" InputGestureText="Ctrl+O" />
                    <Separator />
                    <MenuItem Header="退出" />
                </MenuItem>
            </Menu>
            """;

        public const string ContextMenuDemo = """
            <!-- 附加到任意元素的 ContextMenu；菜单项用原生 MenuItem 或 jv:ContextMenuItem -->
            <jv:Button Content="右键打开上下文菜单" HorizontalAlignment="Left">
                <jv:Button.ContextMenu>
                    <jv:ContextMenu>
                        <jv:ContextMenuItem Header="打开" Icon="&#xE60F;" InputGestureText="Ctrl+O"
                                            Command="{Binding PanelToggleCommand, Source={x:Static sample:SampleData.Instance}}" />
                        <jv:ContextMenuItem Header="保存" Icon="&#xE611;" InputGestureText="Ctrl+S" />
                        <Separator />
                        <jv:ContextMenuItem Header="自动滚动" IsCheckable="True" IsChecked="True" />
                        <jv:ContextMenuItem Header="导出">
                            <jv:ContextMenuItem Header="导出为 PNG" />
                            <jv:ContextMenuItem Header="导出为 JPEG" />
                        </jv:ContextMenuItem>
                    </jv:ContextMenu>
                </jv:Button.ContextMenu>
            </jv:Button>
            """;

        public const string TabControlDemo = """
            <!-- 可关闭页签：IsClosable 控制关闭按钮（默认 True）；双击标题可重命名（控件级 CanRename 默认 False，需显式开启）；
                 CanCloseLastTab=False 保护最后一个页签；TabClosing 事件可取消关闭 -->
            <jv:TabControl Height="220" CanRename="True" CanCloseLastTab="False" TabClosing="OnTabClosing">
                <jv:TabControlItem Header="相机 1" Icon="&#xE66B;">
                    <TextBlock Margin="16" Text="相机 1 的内容区域。" />
                </jv:TabControlItem>
                <jv:TabControlItem Header="日志">
                    <TextBlock Margin="16" Text="日志页签内容。" />
                </jv:TabControlItem>
                <jv:TabControlItem Header="保护页">
                    <TextBlock Margin="16" Text="尝试关闭此页签：TabClosing 会取消并弹出提示。" />
                </jv:TabControlItem>
            </jv:TabControl>
            """;

        public const string TabControlWrapDemo = """
            <!-- 页签条是 WrapPanel 且不套滚动容器：超出宽度自动换行，行数不设上限。
                 换行与横向滚动互斥（ScrollViewer 的 HorizontalScrollBarVisibility=Auto 会给面板无限宽度，
                 WrapPanel 在无限宽度下永远只排一行），所以这里没有横向滚动条。
                 要限制头部占用：给控件 Height/MaxHeight，或外层再套宿主自己的 ScrollViewer -->
            <jv:TabControl Width="420" Height="200" IsClosable="False">
                <jv:TabControlItem Header="总览">
                    <TextBlock Margin="16" Text="总览页签内容。" />
                </jv:TabControlItem>
                <jv:TabControlItem Header="设备">
                    <TextBlock Margin="16" Text="设备页签内容。" />
                </jv:TabControlItem>
                <jv:TabControlItem Header="告警">
                    <TextBlock Margin="16" Text="告警页签内容。" />
                </jv:TabControlItem>
                <jv:TabControlItem Header="录像">
                    <TextBlock Margin="16" Text="录像页签内容。" />
                </jv:TabControlItem>
                <jv:TabControlItem Header="门禁">
                    <TextBlock Margin="16" Text="门禁页签内容。" />
                </jv:TabControlItem>
                <jv:TabControlItem Header="网络">
                    <TextBlock Margin="16" Text="网络页签内容。" />
                </jv:TabControlItem>
                <jv:TabControlItem Header="存储">
                    <TextBlock Margin="16" Text="存储页签内容。" />
                </jv:TabControlItem>
                <jv:TabControlItem Header="系统">
                    <TextBlock Margin="16" Text="系统页签内容。" />
                </jv:TabControlItem>
            </jv:TabControl>
            """;


        // ---------------- 树形视图（并入「菜单与导航」页） ----------------

        public const string TreeViewDemo = """
            <!-- TreeMenuItem 为 POCO 数据模型（Title/Icon/Children），默认 HierarchicalDataTemplate 渲染；
                 展开图标为 ExpanderPanel 同款旋转箭头；NavigateCommand 为选中驱动（单击/键盘/程序化选中即执行，
                 参数=数据项），双击另有 ItemDoubleClick 事件（携带数据项，双击展开箭头不触发）；
                 TreeViewItem 容器不复用（框架默认 Standard），展开/选中状态保存在 TreeMenuItem 数据模型上 -->
            <jv:TreeView x:Name="MainTree" Width="400" Height="420"
                         atc:Icon.FontFamily="{DynamicResource IconFont}"
                         atc:Icon.IconSize="16" />

            <!-- 数据构建（任意线程，POCO 不依赖 UI 线程）：
                 var root = new TreeMenuItem { Title = "检测工作站", Icon = "\uE6BC", IsExpanded = true };
                 root.Children.Add(new TreeMenuItem { Title = "图像采集", Icon = "\uE60C", IsExpanded = true });
                 MainTree.ItemsSource = new[] { root };
                 MainTree.NavigateCommand = new ParameterRelayCommand(node => { ... });
                 注意：NavigateCommand 为选中驱动——选中项变化（单击、键盘、程序化选中）即执行，
                 参数是新选中的节点数据对象（分组/叶子都会发，消费方自滤）；
                 丢弃参数的命令（如库内 Common/RelayCommand.cs 的 RelayCommand(Action)）收不到该节点，
                 示例页自带的 ParameterRelayCommand(Action<object?>) 即为携带参数的宿主侧命令；
                 双击打开类需求请订阅 ItemDoubleClick 路由事件（e.Item 为数据项） -->
            """;

        public const string TreeViewIndicatorDemo = """
            <!-- DisplayMode=Indicator：折叠展开箭头；蓝色 accent 指示条仅标示「选中的带子项节点」
                 （蓝色提示 = 真实选中，非选中分支不显示）；图标字号取 atc:Icon.IconSize，标题仍正常显示。
                 DisplayMode 经 atc:TreeViewAssist.DisplayMode 附加属性承载，官方 <TreeView> 上同样可用 -->
            <jv:TreeView Width="400" Height="300" DisplayMode="Indicator"
                         atc:Icon.FontFamily="{DynamicResource IconFont}"
                         atc:Icon.IconSize="18" />
            """;

        public const string TreeViewNativeDemo = """
            <!-- 合并 Themes/Generic.xaml 后，官方 <TreeView>（无 jv: 前缀）自动获得同一外观；
                 扩展能力经附加属性提供：图标字体、选中画刷、NavigateCommand -->
            <TreeView x:Name="NativeTree" Width="400" Height="300"
                      atc:Icon.FontFamily="{DynamicResource IconFont}"
                      atc:Icon.IconSize="16"
                      atc:TreeViewAssist.SelectedItemBackground="{DynamicResource Theme.Brush.Surface.Selected}" />

            <!-- 代码设置导航命令（对官方实例同样生效）：
                 Junevy.Controls.AttachedProperties.TreeViewAssist.SetNavigateCommand(
                     NativeTree, new ParameterRelayCommand(p => ...)); -->
            """;

        public const string TreeViewInteractionDemo = """
            <!-- TreeMenuItem.IsExpanded / IsSelected 与容器双向绑定，代码直接赋值即可控制；
                 悬停/选中背景画刷逐实例自定义（默认 Surface.Hover / Surface.Sunken） -->
            <jv:Button Click="OnExpandAllClick" Content="展开全部" />
            <jv:TreeView x:Name="ControlledTree" Width="400" Height="260"
                         ItemHoverBackground="{DynamicResource Theme.Brush.Surface.Hover}"
                         SelectedItemBackground="{DynamicResource Theme.Brush.Surface.Selected}"
                         atc:Icon.FontFamily="{DynamicResource IconFont}" atc:Icon.IconSize="16" />

            <!-- 代码控制：写入数据模型，容器经双向绑定跟随；容器不复用（框架默认 Standard），状态保存在模型上。
                 递归展开/收起走树级 API，未生成的容器同样被覆盖，宿主不必自写递归：
                 ControlledTree.ExpandAll(); ControlledTree.CollapseAll();
                 （收起按 WPF 原生语义把选中上提到最外层被收起的祖先，紧随的 ExpandAll 把选中还原回原节点）
                 AutoExpandAncestors（默认 true）只在祖先容器已生成时生效，收起分支里的叶节点还没有容器，
                 所以宿主沿自己查到的路径先展开祖先、再写选中：
                 for (var i = 0; i < path.Count - 1; i++) path[i].IsExpanded = true;
                 path[^1].IsSelected = true; -->
            """;


        // ---------------- 栏与工具（BarsPage） ----------------

        public const string ToolBarDemo = """
            <!-- 横向工具条；ToolBarItem.Icon 是按钮自身属性，Title 经 ToolTip 展示 -->
            <jv:ToolBar HorizontalAlignment="Left" FontFamily="{DynamicResource IconFont}" FontSize="16">
                <jv:ToolBarItem Icon="&#xE60F;" ToolTip="设置" />
                <jv:ToolBarItem Icon="&#xE611;" ToolTip="刷新" />
                <jv:ToolBarItem Icon="&#xE932;" ToolTip="告警" />
                <jv:ToolBarItem Icon="&#xE60C;" ToolTip="截图" />
            </jv:ToolBar>

            <!-- 纵向工具条 -->
            <jv:ToolBar Width="48" HorizontalAlignment="Left" Orientation="Vertical"
                        FontFamily="{DynamicResource IconFont}" FontSize="16">
                <jv:ToolBarItem Icon="&#xE60F;" ToolTip="设置" />
                <jv:ToolBarItem Icon="&#xE611;" ToolTip="刷新" />
            </jv:ToolBar>
            """;

        public const string ToolboxDemo = """
            <!-- 悬浮工具箱：悬停分组打开 Popup；ToolItem 按住可发起 Copy 拖放（IsDragEnabled 默认 True）；
                 任意时刻最多展开一个分组；图标颜色由 atc:Icon.IconForeground 控制 -->
            <jv:Toolbox HorizontalAlignment="Left">
                <jv:ToolboxItem Icon="&#xE60F;" Title="基础工具">
                    <jv:ToolItem Icon="&#xE611;" Title="刷新" />
                    <jv:ToolItem Icon="&#xE60C;" Title="截图" />
                    <jv:ToolItem Icon="&#xE932;" Title="检测" />
                </jv:ToolboxItem>
                <jv:ToolboxItem Icon="&#xE66B;" Title="视觉工具">
                    <jv:ToolItem Icon="&#xE981;" Title="定位" />
                    <jv:ToolItem Icon="&#xE60F;" Title="标定" />
                </jv:ToolboxItem>
            </jv:Toolbox>
            """;

        public const string CompactToolboxDemo = """
            <!-- 紧凑工具箱：整条约 25 DIP 窄列——容器 MaxWidth=25 兜底，实例级 ItemContainerStyle 指定更宽的
                 项样式时窄列也不会被撑宽（但条目样式内固定 Width/MinWidth 大于约 21 DIP 会被裁剪，
                 自定义项样式建议 BasedOn CompactToolboxItemStyle）；
                 窄列容不下滚动条：条目溢出时滚动条隐藏，滚轮仍可滚动；弹出面板与工具项尺寸不受窄列影响 -->
            <jv:Toolbox
                Style="{StaticResource CompactToolboxStyle}"
                HorizontalAlignment="Left"
                ColumnCount="3"
                PopupWidth="225">
                <jv:ToolboxItem Icon="&#xE60F;" Title="基础工具">
                    <jv:ToolItem Icon="&#xE611;" Title="刷新" />
                    <jv:ToolItem Icon="&#xE60C;" Title="截图" />
                    <jv:ToolItem Icon="&#xE932;" Title="检测" />
                </jv:ToolboxItem>
                <jv:ToolboxItem Icon="&#xE66B;" Title="视觉工具">
                    <jv:ToolItem Icon="&#xE981;" Title="定位" />
                    <jv:ToolItem Icon="&#xE60F;" Title="标定" />
                </jv:ToolboxItem>
                <jv:ToolboxItem Icon="&#xE650;" Title="布局工具">
                    <jv:ToolItem Icon="&#xE60C;" Title="对齐" />
                    <jv:ToolItem Icon="&#xE650;" Title="分布" />
                </jv:ToolboxItem>
            </jv:Toolbox>
            """;

        public const string AppBarExpandableDemo = """
            <!-- Mode=Expandable：最左抽屉开关（设置 Drawer 内容才显示）+ 整条居中的标题 + 右侧最小化/最大化/关闭；
                 抽屉经 Popup 从标题栏下方悬浮展开：点外部收起，再次点开关同样收起（开合防抖），
                 AutoCloseOnDrawerClick=True 时点击抽屉内未处理的左键（如列表项）自动收起。
                 caption 按钮经 SystemCommands 作用于所在窗口；
                 拖动交由宿主 WindowChrome caption 区，建议 CaptionHeight="{Binding ActualHeight, ElementName=Bar}" -->
            <jv:AppBar
                Height="56"
                Mode="Expandable"
                AutoCloseOnDrawerClick="True"
                Content="Junevy Studio"
                atc:Icon.FontFamily="{DynamicResource IconFont}">
                <jv:AppBar.Drawer>
                    <ListBox MinWidth="200">
                        <ListBoxItem Content="首页" IsSelected="True" />
                        <ListBoxItem Content="数据采集" />
                        <ListBoxItem Content="报表中心" />
                        <ListBoxItem Content="系统设置" />
                    </ListBox>
                </jv:AppBar.Drawer>
            </jv:AppBar>
            """;

        public const string InfoBarDemo = """
            <!-- Text 布局:头像(AvatarSource 图片,或名称首字符字标)+ 名称 + 设置按钮(SettingsClick);
                 菜单项在 XAML 中直接编写(作为 Items),AutoCloseOnMenuClick=True 时点击菜单内按钮自动收起 -->
            <jv:InfoBar Width="280" UserName="xuhill07" AutoCloseOnMenuClick="True"
                        SettingsClick="OnInfoBarSettingsClick">
                <jv:Button Content="&#xE60F; 应用设置" HorizontalContentAlignment="Left"
                           Style="{StaticResource NoBorderButtonStyle}" />
                <jv:Button Content="&#xE651; 个人资料" HorizontalContentAlignment="Left"
                           Style="{StaticResource NoBorderButtonStyle}" />
                <jv:Button Content="&#xE639; 退出登录" HorizontalContentAlignment="Left"
                           Style="{StaticResource NoBorderButtonStyle}" />
            </jv:InfoBar>

            <!-- AvatarOnly 布局:仅头像(指定宽度时居中显示),悬停经 ToolTip 显示名称;
                 菜单经 ItemsSource 绑定列表,项外观用 ItemTemplate 定制;菜单与控件等宽 -->
            <jv:InfoBar Width="120" DisplayMode="AvatarOnly" UserName="xuhill07"
                        AvatarSource="pack://application:,,,/Junevy.Controls;component/Resources/Pictures/Author.png"
                        ItemsSource="{Binding InfoBarMenuItems, ElementName=PageRoot}"
                        ItemTemplate="{StaticResource InfoBarMenuItemTemplate}"
                        ButtonBase.Click="OnInfoBarMenuItemClick"
                        SettingsClick="OnInfoBarSettingsClick" />

            <!-- 面板模式:MenuContent 放任意内容(ListBox / UserControl / 复杂布局),
                 宽度默认随内容自适应(不受控件尺寸约束),MenuWidth 可显式定宽,MenuMaxHeight 限制弹层高度 -->
            <jv:InfoBar Width="280" UserName="操作面板" MenuWidth="360" MenuMaxHeight="240"
                        SettingsClick="OnInfoBarSettingsClick">
                <jv:InfoBar.MenuContent>
                    <StackPanel>
                        <TextBlock Margin="8,8,8,4" FontWeight="Bold" Text="快捷操作" />
                        <ListBox BorderThickness="0">
                            <ListBoxItem Content="任务 1:导出检测报告" />
                            <ListBoxItem Content="任务 2:重建索引" />
                        </ListBox>
                    </StackPanel>
                </jv:InfoBar.MenuContent>
            </jv:InfoBar>
            """;


        // ---------------- 布局控件（LayoutPage） ----------------

        public const string ExpanderPanelDemo = """
            <!-- 四方向展开 + 过渡动画（AnimationDuration=0 表示直接切换）；
                 头部使用 ToggleButton，按 Space 也可切换；动画基于 LayoutTransform -->
            <jv:ExpanderPanel Header="展开方向 Down（默认，已展开）"
                              AnimationDuration="0:0:0.25"
                              ToggleCommand="{Binding PanelToggleCommand, Source={x:Static sample:SampleData.Instance}}">
                <TextBlock Text="内容区域。" />
            </jv:ExpanderPanel>

            <jv:ExpanderPanel Header="展开方向 Up（初始折叠）" IsExpanded="False"
                              AnimationDuration="0:0:0.25" ExpandDirection="Up" />

            <jv:ExpanderPanel Width="360" Header="Left（内容向左展开）"
                              AnimationDuration="0:0:0.25" ExpandDirection="Left" />
            <jv:ExpanderPanel Width="360" Header="Right（内容向右展开）"
                              AnimationDuration="0:0:0.25" ExpandDirection="Right" />
            """;

        public const string ExpanderPanelCardDemo = """
            <!-- 卡片模式（DisplayMode=Card）：高头部 = 图标 + 标题(Header)/补充说明(Description) + 右侧扩展槽 + 展开箭头；
                 图标为 iconfont 字形时字体族/字号跟随 atc:Icon.FontFamily / atc:Icon.IconSize（卡片默认 20） -->
            <jv:ExpanderPanel DisplayMode="Card" Icon="&#xE60C;"
                              Header="采集通道配置"
                              Description="展开后配置各通道的触发模式与采样率">
                <TextBlock Text="卡片展开后的内容区。整卡带底色与描边，内容向下展开。" />
            </jv:ExpanderPanel>

            <!-- 右侧扩展槽（HeaderExtra）：其中的开关/按钮独立交互，不会误触发展开；空白处点击仍切换 -->
            <jv:ExpanderPanel DisplayMode="Card" IsExpanded="False" Icon="&#xE60F;"
                              Header="自动曝光"
                              Description="启用后由相机驱动自动调整曝光时间与增益">
                <jv:ExpanderPanel.HeaderExtra>
                    <StackPanel Orientation="Horizontal">
                        <jv:ToggleButton IsChecked="True" />
                        <TextBlock Margin="8,0,0,0" VerticalAlignment="Center" Text="On" />
                    </StackPanel>
                </jv:ExpanderPanel.HeaderExtra>
                <TextBlock Text="曝光策略内容区。" />
            </jv:ExpanderPanel>

            <!-- 无图标/无说明/无右侧控件：相应槽位自动折叠，头部随之变矮 -->
            <jv:ExpanderPanel DisplayMode="Card" Header="仅标题的卡片" IsExpanded="False">
                <TextBlock Text="无图标、无说明、无右侧控件。" />
            </jv:ExpanderPanel>
            """;
        public const string SidePanelDemo = """
            <!-- 作为浮层放入 Grid（不指定 Row/Column 自动跨满）；IsOpen/Toggle() 控制开合，
                 Side 指定停靠边；CloseOnOutsideClick 默认开启，IsBackdropEnabled 显示遮罩；
                 CornerRadius 逐角设置圆角，顺序为左上,右上,右下,左下，不设则取主题圆角 -->
            <Grid Height="220">
                <Border Background="{DynamicResource Theme.Brush.Surface.Base}" /> <!-- 页面内容示意 -->

                <jv:SidePanel x:Name="LeftPanel" Side="Left">
                    <StackPanel Width="240" Margin="20,16">
                        <TextBlock FontWeight="Bold" Text="左侧面板（Side=Left）" />
                        <jv:Button HorizontalAlignment="Left" Click="OnClosePanelClick" Content="关闭面板" />
                    </StackPanel>
                </jv:SidePanel>

                <jv:SidePanel x:Name="RightPanel" Side="Right" IsBackdropEnabled="True">
                    <StackPanel Width="240" Margin="20,16">
                        <TextBlock FontWeight="Bold" Text="右侧面板（带遮罩）" />
                    </StackPanel>
                </jv:SidePanel>

                <!-- 顶栏式抽屉：四角全直角 -->
                <jv:SidePanel x:Name="SquarePanel" Side="Top" CornerRadius="0">
                    <StackPanel Height="56" Margin="20,0" Orientation="Horizontal">
                        <TextBlock VerticalAlignment="Center" FontWeight="Bold" Text="CornerRadius=&quot;0&quot;" />
                    </StackPanel>
                </jv:SidePanel>

                <!-- Side=Right 时把贴住容器右侧的两角设为 0，自由边大圆角 -->
                <jv:SidePanel x:Name="FlushPanel" Side="Right" CornerRadius="18,0,0,18" IsBackdropEnabled="True">
                    <StackPanel Width="240" Margin="20,16">
                        <TextBlock FontWeight="Bold" Text="CornerRadius=&quot;18,0,0,18&quot;" />
                    </StackPanel>
                </jv:SidePanel>
            </Grid>
            """;

        public const string CornerRadiusDemo = """
            <!-- Border.CornerRadius 附加用法：经样式 Setter 设置（逐实例 attribute 写法被编译器拒绝） -->
            <Style x:Key="Corner0Button" BasedOn="{StaticResource {x:Type jv:Button}}" TargetType="{x:Type jv:Button}">
                <Setter Property="Border.CornerRadius" Value="0" />
            </Style>
            <Style x:Key="Corner8Button" BasedOn="{StaticResource {x:Type jv:Button}}" TargetType="{x:Type jv:Button}">
                <Setter Property="Border.CornerRadius" Value="8" />
            </Style>
            <Style x:Key="Corner16Button" BasedOn="{StaticResource {x:Type jv:Button}}" TargetType="{x:Type jv:Button}">
                <Setter Property="Border.CornerRadius" Value="16" />
            </Style>

            <jv:Button Content="圆角 0" Style="{StaticResource Corner0Button}" />
            <jv:Button Content="圆角 8" Style="{StaticResource Corner8Button}" />
            <jv:Button Content="圆角 16" Style="{StaticResource Corner16Button}" />
            """;


        // ---------------- 窗口与对话框（WindowPage） ----------------

        public const string DialogWindowDemo = """
            <!-- DialogWindow 经代码实例化弹出（无边框、圆角、主题化标题栏；
                 未启用 SizeToContent 覆盖时窗口尺寸完全跟随内容；Esc 可关闭） -->
            <jv:Button Click="OnOpenDialogClick" Content="打开对话框" HorizontalAlignment="Left" />

            <!-- 代码弹出：
                 var dialog = new DialogWindow { Title = "演示", Content = new TextBlock { Text = "..." } };
                 dialog.ShowDialog(); -->
            """;

        public const string ProgressBarWindowDemo = """
            <!-- ProgressBarWindow 为纯 C# API（进度对话框，默认环形、无边框可拖动）：
                 var dialog = new ProgressBarWindow { Title = "固件部署", Message = "正在部署固件…", Owner = this };
                 dialog.Show();                       // 或 ShowDialog() 模态
                 dialog.Report(45);                   // 后台线程安全汇报进度
                 dialog.UpdateMessage / UpdateDetail  // 更新主/次说明文本
                 dialog.RequestClose();               // 任务完成由代码关闭（或 CloseAfter(task) 自动关闭）
                 dialog.CloseButtonEnabled = false;   // 禁止用户取消（隐藏关闭按钮并拦截 Esc / Alt+F4）
                 dialog.IsCancelled / Cancelled       // 获知用户是否主动取消了等待 -->
            """;

        // ---------------- 图像（ImagePage） ----------------

        public const string ImageViewerDemo = """
            <!-- 滚轮缩放 0.05x-64x，左键拖动平移，右键菜单保存；
                 代码控制：Viewer.FitToWindow() / Viewer.ActualSize() -->
            <jv:ImageViewer x:Name="Viewer" Height="380"
                            Source="pack://application:,,,/Junevy.Controls;component/Resources/Pictures/camera.png" />
            <jv:Button Click="OnFitToWindowClick" Content="FitToWindow()" />
            <jv:Button Click="OnActualSizeClick" Content="ActualSize()" />
            """;

        public const string TransparentBackgroundDemo = """
            <!-- 透明背景棋盘格资源：随主题换色，方格不随控件尺寸拉伸；必须 DynamicResource 引用 -->
            <UniformGrid Columns="4" Rows="1">
                <Border Height="96" Background="{DynamicResource TransparentBackground.Small}" />
                <Border Height="96" Background="{DynamicResource TransparentBackground}" />
                <Border Height="96" Background="{DynamicResource TransparentBackground.Large}" />
                <Path Height="96" Data="{DynamicResource TransparentBackground.Geometry}"
                      Fill="{DynamicResource Theme.Brush.Accent.Primary}" Stretch="Uniform" />
            </UniformGrid>

            <!-- 左起：Small（4px 方格）/ 默认（8px）/ Large（16px）/ 纯几何自绘 -->
            """;


        // ---------------- 图标字体（IconsPage） ----------------

        public const string IconFontUsage = """
            <!-- FontFamily 资源（库已内置，码点两套完全一致）：
                 {DynamicResource IconFont}        线性/描边 iconfont.ttf
                 {DynamicResource IconFontFilled}  面性/填充 iconfont-filled.ttf -->
            <TextBlock FontFamily="{DynamicResource IconFont}" FontSize="24" Text="&#xE63F;" />
            <TextBlock FontFamily="{DynamicResource IconFontFilled}" FontSize="24" Text="&#xE63F;" />

            <!-- 附带属性用法（默认线性）：atc:Icon.Icon 传图标字符 -->
            <jv:Button atc:Icon.Icon="&#xE63F;" atc:Icon.IconSize="16" Content="保存" />
            <jv:Button atc:Icon.FontFamily="{StaticResource IconFontFilled}"
                       atc:Icon.Icon="&#xE63F;" atc:Icon.IconSize="16" Content="保存" />
            """;

        public const string IconFontApply = """
            <jv:Button atc:Icon.Icon="&#xE63F;" atc:Icon.IconSize="16" Content="保存（线性）" />
            <jv:Button atc:Icon.FontFamily="{StaticResource IconFontFilled}"
                       atc:Icon.Icon="&#xE63F;" atc:Icon.IconSize="16" Content="保存（面性）" />
            """;


        // ---------------- 进度条（ProgressPage） ----------------

        public const string ProgressBarLinearDemo = """
            <!-- Value / Minimum / Maximum 走 WPF 标准管线（注意 Slider 默认 Maximum=10，需显式调大），
                 可与 Slider 等直接绑定 -->
            <StackPanel>
                <jv:ProgressBar ShowProgressText="True" Value="{Binding Value, ElementName=ProgressSlider}" />
                <Slider x:Name="ProgressSlider" Margin="0,12,0,0" Maximum="100" Value="45" />
            </StackPanel>
            """;

        public const string ProgressBarIndeterminateDemo = """
            <!-- IsIndeterminate=True：来回扫动动画，用于百分比未知的等待场景 -->
            <jv:ProgressBar IsIndeterminate="True" />
            """;

        public const string ProgressBarCircularDemo = """
            <!-- ShapeMode=Circular：环形；确定模式按值绘制圆弧、ShowProgressText 显示在圆心，
                 RingThickness 控制弧线宽；不确定模式为持续旋转的四分之一圆弧 -->
            <WrapPanel>
                <jv:ProgressBar
                    Width="56"
                    Height="56"
                    RingThickness="5"
                    ShapeMode="Circular"
                    ShowProgressText="True"
                    Value="{Binding Value, ElementName=ProgressSlider}" />
                <jv:ProgressBar
                    Width="32"
                    Height="32"
                    Margin="24,0,0,0"
                    RingThickness="3"
                    ShapeMode="Circular"
                    Value="{Binding Value, ElementName=ProgressSlider}" />
                <jv:ProgressBar
                    Width="32"
                    Height="32"
                    Margin="24,0,0,0"
                    IsIndeterminate="True"
                    RingThickness="3"
                    ShapeMode="Circular" />
            </WrapPanel>
            """;

        public const string ProgressBarTextFormatDemo = """
            <!-- ProgressTextFormat 中 {0} 为 0-100 整数百分比；不设置时默认 "{0}%" -->
            <jv:ProgressBar
                ProgressTextFormat="已完成 {0}%"
                ShowProgressText="True"
                Value="{Binding Value, ElementName=ProgressSlider}" />
            """;

    }
}
